using FluentValidation;
using System;

namespace Projects.Application.Projects.Commands.RescheduleMilestone
{
	public class RescheduleMilestoneCommandValidator : AbstractValidator<RescheduleMilestoneCommand>
	{
		public RescheduleMilestoneCommandValidator()
		{
			RuleFor(x => x.ProjectId)
				.NotEmpty().WithMessage("ProjectId is required.");

			RuleFor(x => x.MilestoneId)
				.NotEmpty().WithMessage("MilestoneId is required.");

			RuleFor(x => x.NewDueDate)
				.GreaterThan(DateTime.UtcNow).WithMessage("New due date must be in the future.");
		}
	}
}
