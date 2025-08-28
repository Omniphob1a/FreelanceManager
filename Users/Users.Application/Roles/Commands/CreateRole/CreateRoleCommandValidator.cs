using FluentValidation;
using Users.Application.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
	public CreateRoleCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Role name cannot be empty")
			.MaximumLength(50).WithMessage("Role name too long (max 50 chars)");

		RuleFor(x => x.PermissionIds)
			.Must(p => p == null || p.Distinct().Count() == p.Count())
			.WithMessage("PermissionIds must be unique");
	}
}