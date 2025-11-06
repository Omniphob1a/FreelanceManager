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

// ----------------- Получаем PORT от Render -----------------
var portEnv = Environment.GetEnvironmentVariable("PORT");
var port = Environment.GetEnvironmentVariable("PORT");

builder.WebHost.ConfigureKestrel(options =>
{
	options.Listen(IPAddress.Any, int.Parse(port));
});

Console.WriteLine($"[DEBUG] Render PORT env = {port}, Kestrel configured for {port}");

// ----------------- Services -----------------
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// Logging, HttpContext
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

// Persistence / Infrastructure / Application / Api registrations
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

// ----------------- Ранний health endpoint -----------------
app.MapGet("/health", () => Results.Text("OK"));

// Статические метрики / Prometheus
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

			var errorJson = System.Text.Json.JsonSerializer.Serialize(error);
			await context.Response.WriteAsync(errorJson);
		}
	});
});

// ----------------- Выполняем миграции -----------------
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
		throw;
	}
}

// ----------------- Background services стартуют автоматически -----------------

// Лог стартового порта
Console.WriteLine($"[INFO] Application starting. Listening on 0.0.0.0:{port}");

// ----------------- Запуск хоста -----------------
app.Run(); // Блокирующий вызов, Render увидит открытый порт
