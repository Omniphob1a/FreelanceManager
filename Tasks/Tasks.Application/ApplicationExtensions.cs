using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Behaviors;
using Tasks.Application.Interfaces;

namespace Tasks.Application
{
	public static class ApplicationExtensions
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			var assembly = Assembly.GetExecutingAssembly();

			services.AddTransient(
				typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
			services.AddTransient(
				typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
			return services;
		}
	}
}
