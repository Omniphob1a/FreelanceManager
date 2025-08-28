using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Application.Mappings
{
	public class ProjectTaskDtoMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectTask, ProjectTaskDto>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.ProjectId, src => src.ProjectId)
				.Map(dest => dest.Title, src => src.Title)
				.Map(dest => dest.Description, src => src.Description)
				.Map(dest => dest.AssigneeId, src => src.AssigneeId)
				.Map(dest => dest.ReporterId, src => src.ReporterId)
				.Map(dest => dest.Status, src => (int)src.Status)
				.Map(dest => dest.Priority, src => (int)src.Priority)
				.Map(dest => dest.TimeEstimatedTicks, src => (long?)src.TimeEstimated.Ticks)
		        .Map(dest => dest.TimeSpentTicks, src => (long?)src.TimeSpent.Ticks)
				.Map(dest => dest.IsBillable, src => src.IsBillable)
				.Map(dest => dest.DueDate, src => src.DueDate)
				.Map(dest => dest.CreatedAt, src => src.CreatedAt)
				.Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
		}
	}
}
