using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Projects.Api;
using Projects.Application;
using Projects.Application.Interfaces;
using Projects.Infrastructure;
using Projects.Infrastructure.Hangfire;
using Projects.Persistence;
using Projects.Persistence.Data;
using Prometheus;
using System.Net;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApiAuthentication(builder.Configuration);
IdentityModelEventSource.ShowPII = true;

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

// OpenAPI endpoint - перенесено выше остальных middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Freelance Projects API v1");
    c.RoutePrefix = "swagger";
});

// Временно отключаем HTTPS редирект для теста
// app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.None,
    Secure = CookieSecurePolicy.Always
});

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/mydashboard", new DashboardOptions
{
    DashboardTitle = "Projects Jobs Dashboard",
    StatsPollingInterval = 5000
});

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

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
    HangfireInitializer.InitializeRecurringJobs(jobManager);
}

app.UseHttpMetrics();

app.MapControllers();

app.MapMetrics();

// Вывод информации о доступных эндпоинтах
Console.WriteLine("Available endpoints:");
Console.WriteLine($"- Swagger UI: http://localhost:5001/swagger");
Console.WriteLine($"- OpenAPI JSON: http://localhost:5001/openapi/v1.json");
Console.WriteLine($"- Hangfire Dashboard: http://localhost:5001/mydashboard");

app.Run();
