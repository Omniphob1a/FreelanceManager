using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Kafka
{
	public class ConfluentConfigOptions
	{
		public string Acks { get; init; } = "all";
		public bool EnableIdempotence { get; init; } = true;
	}
}
