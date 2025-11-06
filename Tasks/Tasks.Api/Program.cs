// Файл: Tasks.Api/Program.cs
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

// --------- get PORT and configure Kestrel + UseUrls robustly ----------
var portEnv = Environment.GetEnvironmentVariable("PORT");
var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var port))
{
	// Bind Kestrel explicitly and also set the server URLs (helps external port scanners)
	builder.WebHost.ConfigureKestrel(options =>
	{
		options.ListenAnyIP(port); // 0.0.0.0:PORT
	});
	builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
	Console.WriteLine($"[DEBUG] Kestrel configured to ListenAnyIP({port}) and UseUrls(http://0.0.0.0:{port}) from PORT env.");
}
else if (!string.IsNullOrEmpty(aspnetcoreUrls))
{
	// If ASPNETCORE_URLS is set to concrete urls (not ${PORT} literal), honor it
	builder.WebHost.UseUrls(aspnetcoreUrls);
	Console.WriteLine($"[DEBUG] UseUrls from ASPNETCORE_URLS: '{aspnetcoreUrls}'.");
}
else
{
	Console.WriteLine("[DEBUG] No explicit PORT/ASPNETCORE_URLS found; using default hosting config.");
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

// ----------------- Run DB migrations (careful: may delay startup) -----------------
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

// ----- TEMP: отключаем https-редирект на Render (включи, если у тебя TLS настроен) -----
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

Console.WriteLine($"[INFO] Tasks.Api starting. PORT='{portEnv ?? "<not set>"}' ASPNETCORE_URLS='{aspnetcoreUrls ?? "<not set>"}'");

app.Run();
