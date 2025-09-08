using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects.Persistence.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Application.Mappings;
using Tasks.Domain.Interfaces;
using Tasks.Persistence.Data;
using Tasks.Persistence.Data.Repositories;
using Tasks.Persistence.Mappings;


namespace Tasks.Persistence
{
	public static class PersistenceExtensions
	{
		public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString(nameof(ProjectTasksDbContext))
				?? throw new ArgumentNullException("Connection string 'ProjectTasksDbContext' not found.");

			services.AddDbContext<ProjectTasksDbContext>(options =>
				options.UseNpgsql(connectionString));

			var config = new TypeAdapterConfig();
			config.Scan(Assembly.GetExecutingAssembly());
			config.Scan(Assembly.GetAssembly(typeof(ProjectTaskDtoMappingConfiguration)));
			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();
			services.AddScoped<ProjectTaskMapper>();
			services.AddScoped<TimeEntryMapper>();
			services.AddScoped<CommentMapper>();

			services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
			services.AddScoped<IProjectReadRepository, ProjectReadRepository>();
			services.AddScoped<IMemberReadRepository, MemberReadRepository>();
			services.AddScoped<IProjectTaskQueryService, ProjectTaskQueryService>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			return services;
		}
	}
}
