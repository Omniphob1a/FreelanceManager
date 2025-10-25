using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Application
{
	public static class ApplicationExtensions
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			var assembly = Assembly.GetExecutingAssembly();
			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

			return services;
		}
	}
}
