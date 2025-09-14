using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;
using Users.Domain.Entities;

namespace Users.Application.Mappings
{
	public class PublicUserDtoMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<User, PublicUserDto>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Login, src => src.Login)
				.Map(dest => dest.Name, src => src.Name)
				.Map(dest => dest.Gender, src => src.Gender)
				.Map(dest => dest.Birthday, src => src.Birthday);
		}
	}
}
