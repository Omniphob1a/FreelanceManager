using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;

namespace Users.Application.Roles.Commands.CreateRole
{
	public record CreateRoleCommand(string Name, IEnumerable<Guid>? PermissionIds = null) : IRequest<Result<RoleDto>>;
}
