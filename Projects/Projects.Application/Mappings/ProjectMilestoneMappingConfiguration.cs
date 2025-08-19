using Mapster;
using Projects.Application.DTOs;
using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Mappings
{
	public class ProjectMilestoneMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectMilestone, ProjectMilestoneDto>()
				.Map(src => src.Id, dest => dest.Id)
				.Map(src => src.Title, dest => dest.Title)
				.Map(src => src.DueDate, dest => dest.DueDate)
				.Map(src => src.IsCompleted, dest => dest.IsCompleted)
				.Map(src => src.IsEscalated, dest => dest.IsEscalated);
		}
	}
}
