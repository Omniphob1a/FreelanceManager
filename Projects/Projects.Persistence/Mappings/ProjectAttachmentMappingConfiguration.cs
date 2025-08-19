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
				.MapWith(src => ProjectAttachment.Load(src.Id, src.FileName, src.Url, src.ProjectId));


			config.NewConfig<ProjectAttachment, ProjectAttachmentEntity>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Url, src => src.Url)
				.Map(dest => dest.FileName, src => src.FileName)
				.Map(dest => dest.ProjectId, src => src.ProjectId);
		}
	}
}
