using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Infrastructure.Kafka;

namespace Notifications.Infrastructure
{
	public static class InfrastructureExtensions
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			var kafkaSection = configuration.GetSection("Kafka");
			var kafkaSettings = kafkaSection.Get<KafkaSettings>() ?? new KafkaSettings();
			services.AddSingleton(kafkaSettings);
			services.Configure<KafkaSettings>(kafkaSection);

			services.AddHostedService<UsersConsumerHostedService>();


			return services;
		}
	}
}
