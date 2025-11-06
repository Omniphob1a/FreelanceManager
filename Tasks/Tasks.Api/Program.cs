// Файл: Tasks.Api/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Tasks.Api;
using Tasks.Application;
using Tasks.Infrastructure;
using Tasks.Persistence;
using Tasks.Persistence.Data;
using HotChocolate;
using HotChocolate.AspNetCore;
using Tasks.Api.GraphQL.Types;
using Tasks.Api.GraphQL.Queries;
using Tasks.Api.GraphQL.Mutations;
using Tasks.Api.GraphQL.DataLoaders;
using Prometheus;
using Projects.Api;

var builder = WebApplication.CreateBuilder(args);

// Render задаёт обязательный порт через переменную окружения PORT
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrEmpty(portEnv))
	throw new Exception("PORT environment variable not set by Render!");

if (!int.TryParse(portEnv, out var port))
	throw new Exception($"Invalid PORT value: {portEnv}");

// Настраиваем Kestrel на прослушку Render порта
builder.WebHost.ConfigureKestrel(options =>
{
	options.ListenAnyIP(port); // 0.0.0.0:PORT
});
Console.WriteLine($"[DEBUG] ConfigureKestrel ListenAnyIP({port})");

// Add services to the container
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

// --- Важно: сначала запускаем Kestrel, чтобы Render увидел открытый порт
await app.StartAsync();
Console.WriteLine($"[DEBUG] Kestrel started, now running migrations (if any)");

// Синхронная миграция БД после старта сервера
using (var scope = app.Services.CreateScope())
{
	try
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();
		dbContext.Database.Migrate();
		Console.WriteLine("[DEBUG] Database migrations applied.");
	}
	catch (Exception ex)
	{
		var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
		logger.LogError(ex, "Database migration failed on startup.");
		throw;
	}
}

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

// В Render HTTPS обычно не нужен
// app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.Strict,
	HttpOnly = HttpOnlyPolicy.None,
	Secure = CookieSecurePolicy.Always
});

app.UseRouting();

app.UseCors("AllowFrontend");

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

app.MapGet("/", () => "OK");

Console.WriteLine($"Listening on port: {port} (Render)");

// Теперь ждем остановки приложения
await app.WaitForShutdownAsync();
