using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Roles.Commands.AddPermissionToRole
{
	public class AddPermissionToRoleCommandValidator : AbstractValidator<AddPermissionToRoleCommand>
	{
		public AddPermissionToRoleCommandValidator()
		{
			RuleFor(x => x.RoleId).NotEmpty();
			RuleFor(x => x.PermissionId).NotEmpty();
		}
	}
}
