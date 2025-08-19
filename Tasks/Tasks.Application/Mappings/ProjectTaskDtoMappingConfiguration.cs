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
				.Map(dest => dest.Status, src => (int)src.Status)
				.Map(dest => dest.EstimateValue, src => src.Estimate.Value)
				.Map(dest => dest.EstimateUnit, src => (int)src.Estimate.Unit)
				.Map(dest => dest.Amount, src => src.HourlyRate.Amount)
				.Map(dest => dest.Currency, src => src.HourlyRate.Currency)
				.Map(dest => dest.IsBillable, src => src.IsBillable)
				.Map(dest => dest.DueDate, src => src.DueDate)
				.Map(dest => dest.CreatedAt, src => src.CreatedAt)
				.Map(dest => dest.CreatedBy, src => src.CreatedBy);	
		}
	}
}
