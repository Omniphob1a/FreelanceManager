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

// ----------------- Получаем PORT от платформы (Render) -----------------
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var port))
{
	// Явно сказать среде, куда слушать (полезно, если платформа смотрит на ASPNETCORE_URLS)
	Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

	// Настроим Kestrel напрямую (дополнительно)
	builder.WebHost.ConfigureKestrel(options =>
	{
		options.ListenAnyIP(port); // 0.0.0.0:PORT
	});
	Console.WriteLine($"[DEBUG] Kestrel configured to ListenAnyIP({port}) and ASPNETCORE_URLS=http://0.0.0.0:{port}");
}
else
{
	Console.WriteLine("[DEBUG] PORT env not set or invalid — using default Kestrel configuration");
}

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

// ----------------- Ранний health endpoint и минимальные маппинги -----------------
// Регистрируем маленький маршрут /health, который ответит сразу — это помогает платформе видеть, что сервис живёт
app.MapGet("/health", () => Results.Text("OK"));

// Статические метрики/прометеус и минимальные маппинги — тоже делаем до долгой инициализации
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseHttpMetrics();
app.MapMetrics();
app.MapControllers();

// Swagger UI (не мешает)
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

// Cookie policy, auth и т.д. — регистрируем middleware
app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.Strict,
	HttpOnly = HttpOnlyPolicy.None,
	Secure = CookieSecurePolicy.Always
});
app.UseAuthorization();

// Global exception handler — оставляем как у вас
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

// ----------------- ВАЖНО: старт хоста до длительной работы -----------------
// StartAsync откроет сокеты и поднимет web host, но не заблокирует поток навсегда.
// Render будет сканить контейнер и увидит открытый порт, даже если дальше идут миграции.
try
{
	Console.WriteLine("[INFO] Starting host (StartAsync) to bind sockets early...");

	await app.StartAsync();
	Console.WriteLine("[INFO] Host started (sockets should be bound).");
}
catch (Exception ex)
{
	Console.WriteLine($"[ERROR] Failed to start host early: {ex}");
	throw;
}

// ----------------- Выполняем тяжелую инициализацию после биндинга -----------------
// Синхронные миграции — выполняем уже после StartAsync, чтобы сокет был виден платформе
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
		// Если миграции критичные — корректно останавливаем хост и выбрасываем
		Console.WriteLine("[ERROR] Migration failed — stopping host.");
		await app.StopAsync();
		throw;
	}
}

// Любые background services / подписчики можно также инициализировать здесь (после миграций).
// Например — ваша инициализация Kafka consumers / Outbox publisher уже зарегистрирована как HostedService и стартует автоматически.

// Зарегистрируем лог стартового порта для удобства
var effectivePortLog = portEnv ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "unknown";
Console.WriteLine($"[INFO] Application started. PORT env = '{portEnv ?? "not set"}', ASPNETCORE_URLS = '{Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}'");

// ----------------- Даем хосту ждать завершения --------------------------------
// Теперь ждем выключения — хост уже запущен и порт открыт.
await app.WaitForShutdownAsync();
