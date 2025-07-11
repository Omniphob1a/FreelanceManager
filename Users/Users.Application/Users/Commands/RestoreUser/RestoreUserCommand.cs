using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.RestoreUser
{
	public record RestoreUserCommand(
		Guid UserId,
		string ModifiedBy
	) : IRequest<Result>;
}
