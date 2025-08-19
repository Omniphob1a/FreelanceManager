using Polly;
using Tasks.Application.Interfaces;
using Tasks.Infrastructure.Services;

namespace Tasks.Api
{
	public static class ApiExtensions
	{
		public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
		{
			services.AddHttpClient<IProjectService, ProjectService>(client =>
			{
				client.BaseAddress = new Uri(config["ProjectService:BaseUrl"]); 
				client.Timeout = TimeSpan.FromSeconds(5);
			})
			.AddTransientHttpErrorPolicy(policyBuilder =>
				policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt)));
			return services;
		}
	}
}
