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
	public class ProjectMemberMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<ProjectMemberEntity, ProjectMember>()
				.MapWith(src => ProjectMember.Load(src.Id, src.UserId, src.Role, src.ProjectId, src.AddedAt));

			config.NewConfig<ProjectMember, ProjectMemberEntity>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.ProjectId, src => src.ProjectId)
				.Map(dest => dest.UserId, src => src.UserId)
				.Map(dest => dest.Role, src => src.Role)
				.Map(dest => dest.AddedAt, src => src.AddedAt);
		}
	}
}
