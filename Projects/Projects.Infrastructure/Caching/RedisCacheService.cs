using Microsoft.Extensions.Caching.Distributed;
using Projects.Application.Common.Abstractions;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Projects.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
	private readonly IDistributedCache _redis;
	private readonly IConnectionMultiplexer _connection;
	private readonly JsonSerializerOptions _jsonOptions;


	public RedisCacheService(IDistributedCache redis, IConnectionMultiplexer connection)
	{
		_redis = redis;
		_connection = connection;

		_jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			IncludeFields = true,
			ReferenceHandler = ReferenceHandler.Preserve,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			TypeInfoResolver = new DefaultJsonTypeInfoResolver()
		};
	}

	public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
	{
		var str = await _redis.GetStringAsync(key, ct);
		return str is null ? default : JsonSerializer.Deserialize<T>(str, _jsonOptions);
	}

	public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
		=> _redis.SetStringAsync(key,
			JsonSerializer.Serialize(value, _jsonOptions),
			new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);

	public Task RemoveAsync(string key, CancellationToken ct)
		=> _redis.RemoveAsync(key, ct);

	public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(prefix))
			throw new ArgumentException("Prefix must not be empty.", nameof(prefix));

		var endpoints = _connection.GetEndPoints();

		foreach (var endpoint in endpoints)
		{
			var server = _connection.GetServer(endpoint);
			if (!server.IsConnected || server.IsReplica) continue;

			var keys = server.Keys(pattern: $"{prefix}*", pageSize: 500);
			var keyList = keys.ToArray();

			if (keyList.Length > 0)
			{
				await _connection.GetDatabase().KeyDeleteAsync(keyList);
			}
		}
	}
}
