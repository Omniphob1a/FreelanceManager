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
	public class UserDtoMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<User, UserDto>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Login, src => src.Login)
				.Map(dest => dest.Name, src => src.Name)
				.Map(dest => dest.Gender, src => src.Gender)
				.Map(dest => dest.Birthday, src => src.Birthday)
				.Map(dest => dest.Email, src => src.Email.Value)
				.Map(dest => dest.Admin, src => src.Admin)
				.Map(dest => dest.CreatedAt, src => src.CreatedAt)
				.Map(dest => dest.ModifiedOn, src => src.ModifiedOn)
				.Map(dest => dest.RevokedOn, src => src.RevokedOn)
				.Ignore(dest => dest.Roles)
				.Ignore(dest => dest.Permissions);
		}
	}
}
