using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Behaviors
{
	public class CachingBehavior<TReq, TRes> : IPipelineBehavior<TReq, TRes>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<CachingBehavior<TReq, TRes>> _log;

		public CachingBehavior(ICacheService cache, ILogger<CachingBehavior<TReq, TRes>> log)
			=> (_cache, _log) = (cache, log);

		public async Task<TRes> Handle(
			TReq request,
			RequestHandlerDelegate<TRes> next,
			CancellationToken ct)
		{
			if (request is not ICacheableQuery cq)
				return await next();

			if (await _cache.GetAsync<TRes>(cq.CacheKey, ct) is { } hit)
			{
				_log.LogDebug("Cache hit {Key}", cq.CacheKey);
				return hit;
			}

			var response = await next();

			var ok = response switch            
			{
				Result r => r.IsSuccess,
				Result<object> ro => ro.IsSuccess,
				_ => true       
			};

			if (ok)
				await _cache.SetAsync(cq.CacheKey, response, cq.Ttl, ct);

			return response;
		}
	}

}
