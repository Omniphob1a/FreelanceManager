using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Roles.Commands.DeleteRole
{
	public record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;
}
