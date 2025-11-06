// File: Tasks.Api/Program.cs
using HotChocolate;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Projects.Api;
using Prometheus;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Tasks.Api;
using Tasks.Api.GraphQL.DataLoaders;
using Tasks.Api.GraphQL.Mutations;
using Tasks.Api.GraphQL.Queries;
using Tasks.Api.GraphQL.Types;
using Tasks.Application;
using Tasks.Infrastructure;
using Tasks.Persistence;
using Tasks.Persistence.Data;

var builder = WebApplication.CreateBuilder(args);

// ----------------- PORT / binding setup -----------------
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrEmpty(portEnv))
{
	// Render usually supplies PORT automatically. Fallback for local runs.
	portEnv = "10000";
	Console.WriteLine("[DEBUG] PORT env not set, using fallback 10000 (local)");
}

if (!int.TryParse(portEnv, out var port))
{
	throw new Exception($"Invalid PORT value: {portEnv}");
}

// Ensure ASPNETCORE_URLS set programmatically to avoid mismatch
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

// Basic bind logs
Console.WriteLine($"[BIND-INFO] Effective PORT: {port}");
Console.WriteLine($"[BIND-INFO] ASPNETCORE_URLS = {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");
Console.WriteLine($"[BIND-INFO] ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? builder.Environment.EnvironmentName}");

// Configure Kestrel and UseUrls as double insurance
builder.WebHost.ConfigureKestrel(options =>
{
	options.ListenAnyIP(port); // 0.0.0.0:PORT
});
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

Console.WriteLine($"[BIND-INFO] Kestrel configured to ListenAnyIP({port}) and UseUrls(http://0.0.0.0:{port})");

// ----------------- Services -----------------
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

// CORS
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

// ----------------- Minimal middleware config (keeps your middlewares) -----------------
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

// Swagger
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Freelance Tasks API v1");
	c.RoutePrefix = "swagger";
});

// Global exception handler (same as before)
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

app.MapGet("/", () => "OK");

// ----------------- Start host early so port is bound ASAP -----------------
try
{
	// Start listening (non-blocking); this opens sockets so Render's scanner can see them.
	Console.WriteLine("[BIND-INFO] Starting host (StartAsync) to ensure sockets are bound before long startup work...");
	var startTask = app.StartAsync();

	// Wait a short time for start to complete binding (but don't block forever)
	var completed = Task.WhenAny(startTask, Task.Delay(TimeSpan.FromSeconds(8))).Result;
	if (completed == startTask && startTask.IsCompletedSuccessfully)
	{
		Console.WriteLine("[BIND-INFO] Host StartAsync completed quickly.");
	}
	else
	{
		Console.WriteLine("[BIND-INFO] StartAsync did not complete within timeout; server may still be binding in background.");
	}

	// Log IServerAddressesFeature (may be empty when using ListenAnyIP, but try)
	var addressesFeature = app.Services.GetService(typeof(IServerAddressesFeature)) as IServerAddressesFeature;
	if (addressesFeature != null && addressesFeature.Addresses != null && addressesFeature.Addresses.Count > 0)
	{
		Console.WriteLine("[BIND-INFO] Server listening addresses (IServerAddressesFeature):");
		foreach (var addr in addressesFeature.Addresses)
			Console.WriteLine($"[BIND-INFO]  - {addr}");
	}
	else
	{
		Console.WriteLine("[BIND-INFO] IServerAddressesFeature reports NO addresses (this is expected when Kestrel configured programmatically).");
	}

	// Extra network diagnostics
	try
	{
		Console.WriteLine("[NET-STATE] Network interfaces:");
		foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
		{
			try
			{
				Console.WriteLine($"[NET-STATE] - {ni.Name} ({ni.OperationalStatus})");
				foreach (var addr in ni.GetIPProperties().UnicastAddresses)
				{
					Console.WriteLine($"[NET-STATE]   -> {addr.Address}");
				}
			}
			catch { /* continue */ }
		}

		var ipProps = IPGlobalProperties.GetIPGlobalProperties();
		var listeners = ipProps.GetActiveTcpListeners();
		Console.WriteLine("[NET-STATE] Active TCP listeners:");
		foreach (var l in listeners)
			Console.WriteLine($"[NET-STATE]  - {l.Address}:{l.Port}");

		var conns = ipProps.GetActiveTcpConnections();
		Console.WriteLine("[NET-STATE] Active TCP connections (sample):");
		foreach (var c in conns.Take(20))
			Console.WriteLine($"[NET-STATE]  - {c.LocalEndPoint} -> {c.RemoteEndPoint} ({c.State})");
	}
	catch (Exception ex)
	{
		Console.WriteLine("[NET-STATE] Failed to enumerate Tcp listeners/connections: " + ex.Message);
	}
}
catch (Exception ex)
{
	Console.WriteLine("[ERROR] Exception while starting host: " + ex.ToString());
	throw;
}

// ----------------- Run DB migrations in background so they don't block port binding -----------------
_ = Task.Run(async () =>
{
	try
	{
		Console.WriteLine("[MIGRATE] Starting DB migrations in background...");
		using var scope = app.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();
		dbContext.Database.Migrate();
		Console.WriteLine("[MIGRATE] DB migrations completed.");
	}
	catch (Exception ex)
	{
		// Log but don't crash the process here (we already started the host and bound sockets).
		var loggerFactory = app.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
		loggerFactory?.CreateLogger("Program").LogError(ex, "Background DB migration failed.");
		Console.WriteLine("[MIGRATE] Background DB migration failed: " + ex.ToString());
	}
});

// ----------------- Start any hosted background services are already wired by builder.Build().
// At this point app has been started, sockets should be bound, Render's scanner can detect port.
// -----------------

Console.WriteLine($"[INFO] Tasks.Api started (or starting). PORT='{portEnv}' ASPNETCORE_URLS='{Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}'");

// Wait for shutdown (normal blocking run)
await app.WaitForShutdownAsync();
