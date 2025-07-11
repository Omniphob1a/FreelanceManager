using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Projects.Api
{
	public static class SwaggerExtensions
	{
		public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
		{
			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Freelance Projects API",
					Description = "API для управления проектами фрилансеров",
					Contact = new OpenApiContact
					{
						Name = "Support",
						Email = "support@freelanceapp.com"
					}
				});

				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Введите JWT токен в формате: Bearer {ваш токен}"
				});

				options.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						Array.Empty<string>()
					}
				});

				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				if (File.Exists(xmlPath))
				{
					options.IncludeXmlComments(xmlPath);
				}
			});

			return services;
		}
	}
}
