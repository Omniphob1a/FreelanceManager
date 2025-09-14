namespace Users.Infrastructure.Kafka
{
	public class ConfluentConfigOptions
	{
		public string Acks { get; init; } = "all";
		public bool EnableIdempotence { get; init; } = true;
	}
}
