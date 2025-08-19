using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy;
using System.Text;
using Yarp.ReverseProxy.Transforms;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
var secretKey = jwtSettings["SecretKey"];

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
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy
			.WithOrigins(
				"http://localhost:8080",   // фронтенд внутри Docker при пробросе 8080
				"http://localhost:5000"    // фронтенд локально через gateway
			)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});
builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
	.AddTransforms(builderContext =>
	{
		builderContext.AddRequestTransform(transformContext =>
		{
			if (transformContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
			{
				var authHeaderString = authHeaderValues.ToString();

				if (!string.IsNullOrEmpty(authHeaderString) && authHeaderString.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				{
					if (AuthenticationHeaderValue.TryParse(authHeaderString, out var authHeaderValue))
					{
						transformContext.ProxyRequest.Headers.Authorization = authHeaderValue;
					}
				}
			}
			return ValueTask.CompletedTask;
		});
	});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

// вот здесь включаем CORS
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
