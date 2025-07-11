using Mapster;
using Projects.Domain.Entities;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Mappings
{
	public class ProjectAttachmentMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectAttachmentEntity, ProjectAttachment>()
				.MapWith(src => new ProjectAttachment(src.Url, src.FileName));

			config.NewConfig<ProjectAttachment, ProjectAttachmentEntity>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Url, src => src.Url)
				.Map(dest => dest.FileName, src => src.FileName);
		}
	}
}
