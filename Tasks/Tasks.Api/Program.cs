// Τΰιλ: Tasks.Api/Program.cs
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

// ----------------- Handle PORT / ASPNETCORE_URLS robustly -----------------
var portEnv = Environment.GetEnvironmentVariable("PORT");
var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

// If PORT exists and is a number — explicitly bind Kestrel to that port (0.0.0.0)
if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var port))
{
	builder.WebHost.ConfigureKestrel(options =>
	{
		options.ListenAnyIP(port); // bind to 0.0.0.0:PORT
	});
	Console.WriteLine($"[DEBUG] Kestrel configured to ListenAnyIP({port}) from PORT env.");
}
// If PORT absent but ASPNETCORE_URLS present and contains ${PORT}, try to expand
else if (!string.IsNullOrEmpty(aspnetcoreUrls) && aspnetcoreUrls.Contains("${PORT}") && !string.IsNullOrEmpty(portEnv))
{
	var expanded = aspnetcoreUrls.Replace("${PORT}", portEnv);
	builder.WebHost.UseUrls(expanded);
	Console.WriteLine($"[DEBUG] Expanded ASPNETCORE_URLS and UseUrls('{expanded}').");
}
else if (!string.IsNullOrEmpty(aspnetcoreUrls))
{
	// If ASPNETCORE_URLS is set to a concrete url(s), honor it
	builder.WebHost.UseUrls(aspnetcoreUrls);
	Console.WriteLine($"[DEBUG] UseUrls from ASPNETCORE_URLS: '{aspnetcoreUrls}'.");
}
else
{
	Console.WriteLine("[DEBUG] No explicit PORT / ASPNETCORE_URLS; using default host config.");
}

// ----------------- Add services -----------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

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

// ----------------- Migrations -----------------
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

// ----------------- Swagger -----------------
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

// ----------------- Global exception handler -----------------
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

// Final log so we see what we started with
Console.WriteLine($"[INFO] Tasks.Api starting. PORT='{portEnv ?? "<not set>"}' ASPNETCORE_URLS='{aspnetcoreUrls ?? "<not set>"}'");

app.Run();
