using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Projects.Domain.Repositories;
using Projects.Persistence.Data;
using Projects.Persistence.Mappings;
using Projects.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence
{
	public static class PersistenceExtensions
	{
		public static IServiceCollection AddPersistence(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddDbContext<ProjectsDbContext>(options =>
				options.UseNpgsql
				(
					configuration.GetConnectionString(nameof(ProjectsDbContext))
				)
			);

			var config = new TypeAdapterConfig();

			new ProjectMappingConfiguration().Register(config);
			new ProjectMilestoneMappingConfiguration().Register(config);
			new ProjectAttachmentMappingConfiguration().Register(config);

			services.AddSingleton(config);

			services.AddScoped<IProjectRepository, ProjectRepository>();

			services.AddScoped<IMapper>(sp =>
			{
				var cfg = sp.GetRequiredService<TypeAdapterConfig>();
				return new Mapper(cfg);
			});

			return services;
		}
	}
}
