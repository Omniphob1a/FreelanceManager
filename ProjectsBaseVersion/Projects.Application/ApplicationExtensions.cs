using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Interfaces;
using Projects.Application.Mappings;
using Projects.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application
{
	public static class ApplicationExtensions 
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			var assembly = Assembly.GetExecutingAssembly();

			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

			var config = new TypeAdapterConfig();

			new ProjectDtoMappingConfiguration().Register(config);

			services.AddSingleton(config);

			services.AddScoped<IMapper>(sp =>
			{
				var cfg = sp.GetRequiredService<TypeAdapterConfig>();
				return new Mapper(cfg);
			});

			services.AddValidatorsFromAssembly(assembly);
			services.AddScoped<ICurrentUserService, CurrentUserService>();
			return services;
		}
	}
}
