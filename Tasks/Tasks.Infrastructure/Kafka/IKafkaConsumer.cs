using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Infrastructure.Kafka
{
	public interface IKafkaConsumer : IDisposable
	{
		Task ConsumeAsync(string topic, Func<string?, string, Task> handler, CancellationToken ct = default);
	}
}
