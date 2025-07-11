using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Repositories;
using Projects.Persistence.Data;
using Projects.Persistence.Mappings;
using Projects.Persistence.Repositories;
using Projects.Persistence.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Projects.Persistence
{
	public static class PersistenceExtensions
	{
		public static IServiceCollection AddPersistence(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString(nameof(ProjectsDbContext))
				?? throw new ArgumentNullException("Connection string 'ProjectsDbContext' not found.");

			services.AddDbContext<ProjectsDbContext>(options =>
				options.UseNpgsql(connectionString));

			var config = new TypeAdapterConfig();
			config.Scan(Assembly.GetExecutingAssembly());
			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();

			services.AddScoped<IProjectRepository, ProjectRepository>();
			services.AddScoped<IProjectQueryService, ProjectQueryService>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			return services;
		}

	}
}
