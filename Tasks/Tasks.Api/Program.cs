// Tasks.Api/Program.cs
using HotChocolate;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Projects.Api;
using Prometheus;
using System.Net;
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

// ----------------- Получаем PORT от Render -----------------
var portEnv = Environment.GetEnvironmentVariable("PORT");
var port = 0;

if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var parsedPort))
{
	port = parsedPort;
	Console.WriteLine($"[DEBUG] Render PORT env found: {port}");

	// Устанавливаем ASPNETCORE_URLS для Kestrel
	Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

	builder.WebHost.ConfigureKestrel(options =>
	{
		options.ListenAnyIP(port);
	});

	Console.WriteLine($"[DEBUG] Kestrel configured to ListenAnyIP({port})");
}
else
{
	Console.WriteLine("[WARNING] PORT env not set or invalid — using default Kestrel configuration");
}

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

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://localhost:8080", "http://localhost:5000", "http://frontend:80")
			  .AllowAnyMethod()
			  .AllowAnyHeader()
			  .AllowCredentials();
	});
});

var app = builder.Build();

// ----------------- Ранний health endpoint и минимальные маппинги -----------------
app.MapGet("/health", () => Results.Text("OK"));
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseHttpMetrics();
app.MapMetrics();
app.MapControllers();

// Swagger UI
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

// Cookie policy, auth и т.д.
app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.Strict,
	HttpOnly = HttpOnlyPolicy.None,
	Secure = CookieSecurePolicy.Always
});
app.UseAuthorization();

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
			await context.Response.WriteAsJsonAsync(error);
		}
	});
});

// ----------------- Старт хоста до длительной работы -----------------
try
{
	Console.WriteLine("[INFO] Starting host (StartAsync) to bind sockets early...");
	await app.StartAsync();
	Console.WriteLine("[INFO] Host started — sockets bound.");
}
catch (Exception ex)
{
	Console.WriteLine($"[ERROR] Failed to start host early: {ex}");
	throw;
}

// ----------------- Тяжелая инициализация после биндинга -----------------
using (var scope = app.Services.CreateScope())
{
	try
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();
		Console.WriteLine("[INFO] Running database migrations...");
		dbContext.Database.Migrate();
		Console.WriteLine("[INFO] Database migrations completed.");
	}
	catch (Exception ex)
	{
		var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
		logger.LogError(ex, "Database migration failed on startup.");
		Console.WriteLine("[ERROR] Migration failed — stopping host.");
		await app.StopAsync();
		throw;
	}
}

// ----------------- Логируем порт для Render -----------------
Console.WriteLine($"[INFO] Application started. Listening on port: {port}");

await app.WaitForShutdownAsync();
