using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// --- JWT ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
var secretKey = jwtSettings["SecretKey"];

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        RoleClaimType = "role",
    };
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:8080",
            "http://localhost:5000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// --- Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GatewayPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            if (context.Resource is HttpContext httpCtx)
            {
                var path = httpCtx.Request.Path;
                if (path.StartsWithSegments("/api/Auth", StringComparison.OrdinalIgnoreCase))
                    return true;

                return context.User?.Identity?.IsAuthenticated == true;
            }

            return context.User?.Identity?.IsAuthenticated == true;
        });
    });
});

// --- YARP ---
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilder =>
    {
        transformBuilder.AddRequestTransform(transformContext =>
        {
            if (transformContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                var authHeaderString = authHeaderValues.ToString();

                if (!string.IsNullOrEmpty(authHeaderString) &&
                    authHeaderString.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    if (AuthenticationHeaderValue.TryParse(authHeaderString, out var authHeaderValue))
                        transformContext.ProxyRequest.Headers.Authorization = authHeaderValue;
                }
            }

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

app.UseRouting();

// ДАЁМ метрики HTTP/Kestrel
app.UseHttpMetrics();

// !!! Надёжно регистрируем endpoint через endpoint routing:
app.MapMetrics("/metrics");

// затем pipeline
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Map reverse proxy — он не будет перехватывать /metrics
app.MapReverseProxy()
   .RequireAuthorization("GatewayPolicy");

app.MapGet("/health", () => Results.Ok("healthy"));
app.MapGet("/metrics-check", () => Results.Text("metrics ok"));

app.Run();
