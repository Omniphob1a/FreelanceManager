using FluentValidation;

namespace Users.Application.Projects.Commands.ConfirmProject
{
	public class ConfirmProjectCommandValidator : AbstractValidator<ConfirmProjectCommand>
	{
		public ConfirmProjectCommandValidator()
		{
			RuleFor(x => x.ProjectId).NotEmpty().WithMessage("ProjectId is required.");
			RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
		}
	}
}
