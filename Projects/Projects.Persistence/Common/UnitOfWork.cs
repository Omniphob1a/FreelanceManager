using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Common;
using Projects.Domain.Interfaces;
using Projects.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Common
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ProjectsDbContext _dbContext;
		private readonly IDomainEventDispatcher _dispatcher;
		private readonly ILogger<UnitOfWork> _logger;

		public UnitOfWork(
			ProjectsDbContext dbContext,
			IDomainEventDispatcher dispatcher,
			ILogger<UnitOfWork> logger)
		{
			_dbContext = dbContext;
			_dispatcher = dispatcher;
			_logger = logger;
		}

		public async Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
			try
			{
				// 1. Собираем события ДО сохранения
				var eventsToDispatch = new List<IDomainEvent>();
				var aggregates = _dbContext.ChangeTracker.Entries<EntityBase>()
					.Where(e => e.Entity.DomainEvents.Any())
					.Select(e => e.Entity)
					.ToList();

				foreach (var aggregate in aggregates)
				{
					eventsToDispatch.AddRange(aggregate.DomainEvents);
					aggregate.ClearDomainEvents();
				}

				var result = await _dbContext.SaveChangesAsync(ct);

				if (eventsToDispatch.Any())
				{
					await _dispatcher.DispatchAsync(eventsToDispatch.ToArray(), ct);
				}

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save changes");
				throw;
			}
		}
	}
}
