using Projects.Application.Interfaces;
using Projects.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ProjectsDbContext _dbContext;

		public UnitOfWork(ProjectsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
