using Mapster;
using Projects.Domain.Entities.ProjectService.Domain.Entities;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Mappings
{
	public class ProjectMilestoneMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectMilestoneEntity, ProjectMilestone>()
				.MapWith(src => ProjectMilestone.Load(src.Title, src.DueDate, src.IsCompleted, src.ProjectId));

			config.NewConfig<ProjectMilestone, ProjectMilestoneEntity>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Title, src => src.Title)
				.Map(dest => dest.DueDate, src => src.DueDate)
				.Map(dest => dest.IsCompleted, src => src.IsCompleted)
				.Map(dest => dest.ProjectId, src => src.ProjectId);
		}
	}
}
