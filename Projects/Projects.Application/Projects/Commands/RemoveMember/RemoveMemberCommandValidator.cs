using FluentValidation;

namespace Projects.Application.Projects.Commands.RemoveMember;

public class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
	public RemoveMemberCommandValidator()
	{
		RuleFor(x => x.ProjectId)
			.NotEmpty().WithMessage("ProjectId is required.");

		RuleFor(x => x.Email)
			.EmailAddress().WithMessage("Email is required.");
	}
}
