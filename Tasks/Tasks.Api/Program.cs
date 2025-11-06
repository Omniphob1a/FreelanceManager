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

// ??????????????? Получаем порт Render ???????????????
var portEnv = Environment.GetEnvironmentVariable("PORT") ?? "10000";
if (!int.TryParse(portEnv, out var port))
{
	Console.WriteLine($"[WARN] Invalid PORT='{portEnv}', falling back to 10000");
	port = 10000;
}

// Устанавливаем адрес прослушки явно (и UseUrls для совместимости)
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
Console.WriteLine($"[DEBUG] Kestrel configured to ListenAnyIP({port}) and UseUrls(http://0.0.0.0:{port})");

// ??????????????? Services ???????????????
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

// ????????? START the web host (this binds the sockets) ?????????
await app.StartAsync(); // important — binds Kestrel before we run migrations
Console.WriteLine($"[INFO] App.StartAsync() completed — sockets should be bound on port {port}");

// ??????????????? Синхронная миграция БД ???????????????
// Выполним миграции после бинда, чтобы Render точно увидел открытый порт
try
{
	using (var scope = app.Services.CreateScope())
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();
		dbContext.Database.Migrate();
	}
	Console.WriteLine("[INFO] Database migrations completed");
}
catch (Exception ex)
{
	var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
	logger.LogError(ex, "Database migration failed on startup.");
	// если миграция критична — остановим приложение
	await app.StopAsync();
	throw;
}

// ??????????????? Swagger UI ???????????????
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

// ??????????????? Global exception handler ???????????????
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

Console.WriteLine($"[INFO] Tasks.Api started. PORT='{port}' ASPNETCORE_URLS='{Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "<not set>"}'");

// ??????????????? Wait until shutdown ???????????????
await app.WaitForShutdownAsync();
