using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.DeleteUser
{
	public record DeleteUserCommand(
		Guid UserId,
		bool HardDelete,
		string RevokedBy
	) : IRequest<Result>;
}
