using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;

namespace Users.Application.Roles.Commands.RemovePermissionFromRole
{
	public record RemovePermissionFromRoleCommand(Guid RoleId, Guid PermissionId) : IRequest<Result<RoleDto>>;
}
