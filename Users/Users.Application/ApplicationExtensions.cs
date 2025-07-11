using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Users.Application.Users.Commands.RegisterUser;
using Users.Application.Users.Queries.AuthenticateUser;

namespace Users.Application
{
	public static class ApplicationExtensions
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			var assembly = Assembly.GetExecutingAssembly();

			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

			services.AddValidatorsFromAssembly(assembly);

			return services;
		}
	}
}
