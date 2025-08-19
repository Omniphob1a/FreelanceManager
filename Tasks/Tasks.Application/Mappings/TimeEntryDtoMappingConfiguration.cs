using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Domain.Aggregate.Entities;

namespace Tasks.Application.Mappings
{
	public class TimeEntryDtoMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<TimeEntry, TimeEntryDto>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.TaskId, src => src.TaskId)
				.Map(dest => dest.UserId, src => src.UserId)
				.Map(dest => dest.StartedAt, src => src.Period.Start)
				.Map(dest => dest.EndedAt, src => src.Period.End)
				.Map(dest => dest.Description, src => src.Description)
				.Map(dest => dest.Hours, src => (decimal)src.Duration.TotalHours)
				.Map(dest => dest.CreatedAt, src => src.CreatedAt);
		}
	}
}
