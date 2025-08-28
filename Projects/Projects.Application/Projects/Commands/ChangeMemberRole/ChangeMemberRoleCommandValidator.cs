using FluentValidation;

namespace Projects.Application.Projects.Commands.ChangeMemberRole;

public class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
	public ChangeMemberRoleCommandValidator()
	{
		RuleFor(x => x.ProjectId)
			.NotEmpty().WithMessage("ProjectId is required.");

		RuleFor(x => x.UserId)
			.NotEmpty().WithMessage("UserId is required.");

		RuleFor(x => x.NewRole)
			.NotEmpty().WithMessage("Role is required.")
			.MaximumLength(100).WithMessage("Role must not exceed 100 characters.");
	}
}
