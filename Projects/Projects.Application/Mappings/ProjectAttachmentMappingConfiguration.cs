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
	public class ProjectAttachmentMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectAttachment, ProjectAttachmentDto>()
				.Map(src => src.FileName, dest => dest.FileName)
				.Map(src => src.Url, dest => dest.Url);
		}
	}
}
