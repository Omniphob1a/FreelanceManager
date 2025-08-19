// ============================
// Projects.Persistence.Common
// ============================

using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Common;
using Projects.Domain.Interfaces;
using Projects.Persistence.Data;

namespace Projects.Persistence.Common
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ProjectsDbContext _dbContext;
		private readonly IDomainEventDispatcher _dispatcher;
		private readonly ILogger<UnitOfWork> _logger;
		private readonly HashSet<EntityBase> _trackedEntities = new();

		public UnitOfWork(
			ProjectsDbContext dbContext,
			IDomainEventDispatcher dispatcher,
			ILogger<UnitOfWork> logger)
		{
			_dbContext = dbContext;
			_dispatcher = dispatcher;
			_logger = logger;
		}

		public void TrackEntity(EntityBase entity)
		{
			if (!_trackedEntities.Contains(entity))
				_trackedEntities.Add(entity);
		}

		public async Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
			try
			{
				var eventsToDispatch = _trackedEntities
					.SelectMany(e => e.DomainEvents)
					.ToList();

				_logger.LogInformation(
					"Preparing to dispatch {Count} domain events",
					eventsToDispatch.Count);

				var result = await _dbContext.SaveChangesAsync(ct);

				if (eventsToDispatch.Any())
				{
					await _dispatcher.DispatchAsync(eventsToDispatch, ct);
					_logger.LogInformation(
						"Successfully dispatched {Count} domain events",
						eventsToDispatch.Count);
				}
				else
				{
					_logger.LogDebug("No domain events to dispatch");
				}

				foreach (var agg in _trackedEntities)
					agg.ClearDomainEvents();

				_trackedEntities.Clear();

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "UnitOfWork.SaveChangesAsync failed");
				throw;
			}
		}
	}
}
