using FluentValidation;

namespace Projects.Application.Projects.Commands.AddMember;

public class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
	public AddMemberCommandValidator()
	{
		RuleFor(x => x.ProjectId)
			.NotEmpty().WithMessage("ProjectId is required.");

		RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");

		RuleFor(x => x.Role)
			.NotEmpty().WithMessage("Role is required.")
			.MaximumLength(100).WithMessage("Role must not exceed 100 characters.");
	}
}
