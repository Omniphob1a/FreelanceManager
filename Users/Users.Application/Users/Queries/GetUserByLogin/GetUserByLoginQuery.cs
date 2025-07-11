using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;

namespace Users.Application.Users.Queries.GetUserByLogin
{
	public record GetUserByLoginQuery(string Login) : IRequest<UserDto>;
}
