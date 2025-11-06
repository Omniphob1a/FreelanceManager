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
	// Если в Render нет переменной — не создаём её вручную в окружении Render.
	// Но для локальной отладки ставим fallback.
	portEnv = "10000";
	Console.WriteLine("[DEBUG] PORT env not set, using fallback 10000 (local)");
}

if (!int.TryParse(portEnv, out var port))
{
	throw new Exception($"Invalid PORT value: {portEnv}");
}

// Программно задаём ASPNETCORE_URLS, чтобы убрать несоответствия конфигов
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

// На всякий случай логируем важные переменные окружения (без секретов)
Console.WriteLine($"[BIND-INFO] Effective PORT: {port}");
Console.WriteLine($"[BIND-INFO] ASPNETCORE_URLS = {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");
Console.WriteLine($"[BIND-INFO] ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? builder.Environment.EnvironmentName}");

// Настроим Kestrel и UseUrls — двойная гарантия
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

// ----------------- DB migration (sync) -----------------
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

// ----------------- Before Run: show network interfaces (extra diagnostics) -----------------
try
{
	Console.WriteLine("[NET-INFO] Network interfaces:");
	foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
	{
		try
		{
			Console.WriteLine($"[NET-INFO] - {ni.Name} ({ni.OperationalStatus})");
			foreach (var addr in ni.GetIPProperties().UnicastAddresses)
			{
				Console.WriteLine($"[NET-INFO]   -> {addr.Address}");
			}
		}
		catch { /* ignore per-interface errors */ }
	}
}
catch (Exception ex)
{
	Console.WriteLine("[NET-INFO] Failed to enumerate network interfaces: " + ex.Message);
}

// ----------------- START the host programmatically and log actual server addresses -----------------
try
{
	// Start the server but don't block; allows to inspect actual addresses
	var startTask = app.StartAsync();

	// Wait short time for server to start and bind
	Task.WaitAny(new[] { startTask }, TimeSpan.FromSeconds(10));

	// Inspect server addresses feature
	var addressesFeature = app.Services.GetService(typeof(IServerAddressesFeature)) as IServerAddressesFeature;
	if (addressesFeature != null && addressesFeature.Addresses != null && addressesFeature.Addresses.Count > 0)
	{
		Console.WriteLine("[BIND-INFO] Server listening addresses (IServerAddressesFeature):");
		foreach (var addr in addressesFeature.Addresses)
		{
			Console.WriteLine($"[BIND-INFO]  - {addr}");
		}
	}
	else
	{
		Console.WriteLine("[BIND-INFO] IServerAddressesFeature reports NO addresses. That means ASP.NET didn't register addresses via that feature.");
		Console.WriteLine("[BIND-INFO] Confirm that Kestrel ListenAnyIP and ASPNETCORE_URLS are properly set (we set them programmatically).");
		Console.WriteLine("[BIND-INFO] If Render still reports 'No open ports detected', check Render service type (must be Web Service) and that nothing else in the container overrides URLs.");
	}

	Console.WriteLine($"[INFO] Tasks.Api started. PORT='{portEnv}' ASPNETCORE_URLS='{Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}'");
}
catch (Exception ex)
{
	Console.WriteLine("[ERROR] Exception during app.StartAsync(): " + ex.ToString());
	throw;
}

// Теперь блокируем основной поток стандартным wait-for-shutdown, как обычно
await app.WaitForShutdownAsync();
