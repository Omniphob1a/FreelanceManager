// Projects.Application/Common/Behaviors/CachingBehavior.cs
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Projects.Application.Common.Behaviors
{
	public class CachingBehavior<TReq, TRes> : IPipelineBehavior<TReq, TRes>
		where TReq : ICacheableQuery
	{
		private readonly ILogger<CachingBehavior<TReq, TRes>> _log;
		private readonly IDistributedCache _cache;
		private static readonly JsonSerializerOptions _opts = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public CachingBehavior(
			ILogger<CachingBehavior<TReq, TRes>> log,
			IDistributedCache cache)
		{
			_log = log;
			_cache = cache;
		}

		public async Task<TRes> Handle(
			TReq request,
			RequestHandlerDelegate<TRes> next,
			CancellationToken ct)
		{
			if (request.BypassCache)
				return await next();

			var key = request.CacheKey;
			_log.LogInformation("Cache lookup for {Key}", key);

			var json = await _cache.GetStringAsync(key, ct);
			if (json is not null)
			{
				_log.LogInformation("  → HIT {Key}", key);

				if (typeof(TRes).IsGenericType
					&& typeof(TRes).GetGenericTypeDefinition() == typeof(Result<>))
				{
					var payloadType = typeof(TRes).GenericTypeArguments[0];
					var payload = JsonSerializer.Deserialize(json, payloadType, _opts)!;

					var okMethod = typeof(Result)
						.GetMethods()
						.First(m =>
							m.Name == nameof(Result.Ok)
							&& m.IsGenericMethodDefinition
							&& m.GetParameters().Length == 1);

					var okGeneric = okMethod.MakeGenericMethod(payloadType);
					var wrapped = okGeneric.Invoke(null, new object[] { payload })!;

					return (TRes)wrapped;
				}

				return JsonSerializer.Deserialize<TRes>(json, _opts)!;
			}

			_log.LogInformation("  → MISS {Key}", key);

			var response = await next();

			string toCache;
			if (response is IResultBase && response.GetType().GetGenericTypeDefinition() == typeof(Result<>))
			{
				var payload = response.GetType()
					.GetProperty(nameof(Result<object>.Value))!
					.GetValue(response);

				toCache = JsonSerializer.Serialize(payload, _opts);
			}
			else
			{
				toCache = JsonSerializer.Serialize(response, _opts);
			}

			var entryOpts = new DistributedCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromMinutes(request.SlidingExpirationInMinutes))
				.SetAbsoluteExpiration(TimeSpan.FromMinutes(request.AbsoluteExpirationInMinutes));

			await _cache.SetStringAsync(key, toCache, entryOpts, ct);
			_log.LogInformation("  → CACHED {Key}", key);

			return response;
		}
	}
}
