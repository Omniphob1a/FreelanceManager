// Τΰιλ: Tasks.Api/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Projects.Api;
using Prometheus;
using System.Net;
using System.Net.NetworkInformation;
using Tasks.Api;
using Tasks.Application;
using Tasks.Infrastructure;
using Tasks.Persistence;
using Tasks.Persistence.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Configure port robustly ---
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var port))
{
	builder.WebHost.ConfigureKestrel(options =>
	{
		options.ListenAnyIP(port);
	});
	builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
	Console.WriteLine($"[DEBUG] Kestrel configured to ListenAnyIP({port}) and UseUrls(http://0.0.0.0:{port})");
}
else
{
	Console.WriteLine("[DEBUG] PORT not set or invalid - using default hosting config");
}

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApi(builder.Configuration);

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy
			.WithOrigins(
				"http://localhost:8080",
				"http://localhost:5000",
				"http://frontend:80"
			)
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials();
	});
});

var app = builder.Build();

// --- Minimal routes / health ---
app.MapGet("/", () => Results.Text("OK"));
app.MapGet("/health/live", () => Results.Text("alive"));
app.MapGet("/health/ready", () => Results.Text("ready"));

// --- Swagger ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Freelance Tasks API v1");
	c.RoutePrefix = "swagger";
});

// NOTE: temporarily don't force https redirect on Render
// app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.Strict,
	HttpOnly = HttpOnlyPolicy.None,
	Secure = CookieSecurePolicy.Always
});

app.UseAuthorization();
app.UseHttpMetrics();

app.MapControllers();
app.MapMetrics();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
		context.Response.ContentType = "application/json";

		var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerFeature>();
		if (exceptionHandlerPathFeature != null)
		{
			var error = new
			{
				Message = "An unexpected error occurred.",
				Detail = app.Environment.IsDevelopment() ? exceptionHandlerPathFeature.Error.ToString() : null
			};

			var errorJson = System.Text.Json.JsonSerializer.Serialize(error);
			await context.Response.WriteAsync(errorJson);
		}
	});
});


// --- START SERVER IMMEDIATELY, THEN run migrations in background ---
// This is the key change: Start Kestrel (so port is listenable) before heavy migrations.
await app.StartAsync(); // non-blocking start: server will bind socket now

// Log active TCP listeners to help Render debugging
try
{
	var props = IPGlobalProperties.GetIPGlobalProperties();
	var listeners = props.GetActiveTcpListeners();
	Console.WriteLine("[DEBUG] Active TCP listeners:");
	foreach (var l in listeners)
	{
		Console.WriteLine($" - {l.Address}:{l.Port}");
	}
}
catch (Exception ex)
{
	Console.WriteLine("[DEBUG] Failed to enumerate TCP listeners: " + ex);
}

// Run DB migrations in background so startup (port binding) is fast and scanner can see port
_ = Task.Run(() =>
{
	try
	{
		using (var scope = app.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();
			Console.WriteLine("[DEBUG] Running DB migrations (background)...");
			dbContext.Database.Migrate();
			Console.WriteLine("[DEBUG] DB migrations finished.");
		}
	}
	catch (Exception ex)
	{
		var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
		logger.LogError(ex, "Database migration failed in background.");
		// do not rethrow — we want the server to stay up for Render's port detection
	}
});

// Final info and wait for shutdown
Console.WriteLine($"[INFO] Tasks.Api started. PORT='{portEnv ?? "<not set>"}' ASPNETCORE_URLS='{Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "<not set>"}'");

await app.WaitForShutdownAsync();
