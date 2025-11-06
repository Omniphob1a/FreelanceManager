// Файл: Tasks.Api/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Projects.Api;
using Prometheus;
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

var builder = WebApplication.CreateBuilder(args);

// Используем переменную PORT (Render) или локальный fallback
var port = Environment.GetEnvironmentVariable("PORT") ?? "5006"; 
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
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

// Синхронная миграция БД (оставляем старый вариант)
using (var scope = app.Services.CreateScope())
{
	try
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();
		dbContext.Database.Migrate();
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

app.UseHttpsRedirection();

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
Console.WriteLine($"Listening on port: {port}");

app.Run();
