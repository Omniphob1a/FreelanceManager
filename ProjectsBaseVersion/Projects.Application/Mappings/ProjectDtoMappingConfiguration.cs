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
	public class ProjectDtoMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Project, ProjectDto>()
				.Map(dest => dest.BudgetMin, src => src.Budget.Min)
				.Map(dest => dest.BudgetMax, src => src.Budget.Max)
				.Map(dest => dest.Currency, src => src.Budget.Currency)
				.Map(dest => dest.Milestones, src => src.Milestones.Adapt<List<ProjectMilestoneDto>>())
				.Map(dest => dest.Attachments, src => src.Attachments.Adapt<List<ProjectAttachmentDto>>())
				.Map(dest => dest.Status, src => (int)src.Status)
				.Map(dest => dest.ExpiresAt, src => src.ExpiresAt)
				.Map(dest => dest.Tags, src => src.Tags.ToList());
		}
	}
}
