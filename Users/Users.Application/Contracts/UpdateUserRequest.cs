using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Users.Commands.UpdateUser;

namespace Users.Application.Contracts
{
	public record UpdateUserRequest(
		Guid UserId,
		UpdateUserCommand Command
	) : IRequest<Result>;
}
