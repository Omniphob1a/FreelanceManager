using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Common.Behaviors;
using Projects.Application.Interfaces;
using Projects.Application.Mappings;
using Projects.Application.Projects.Commands.AddAttachment;
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

			services.AddValidatorsFromAssembly(assembly);
			services.AddScoped<ICurrentUserService, CurrentUserService>();
			services.AddScoped<TagParserService>();
			services.AddTransient(
				typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
			services.AddTransient(
				typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			return services;
		}
	}
}
