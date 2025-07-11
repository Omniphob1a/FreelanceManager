using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Projects.Shared.Extensions
{
	public static class LoggerExtensions
	{
		public static void LogMappingError<TSource, TDestination>(this ILogger logger, object source, Exception exception)
		{
			logger.LogError(exception,
				"Mapping failed: {SourceType} → {DestinationType}. Source: {@Source}",
				typeof(TSource).Name,
				typeof(TDestination).Name,
				source
			);
		}
	}
}
