using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.UpdateProjectTask
{
	public class UpdateProjectTaskCommandValidator : AbstractValidator<UpdateProjectTaskCommand>
	{
		public UpdateProjectTaskCommandValidator()
		{
			RuleFor(x => x.TaskId)
				.NotEmpty().WithMessage("TaskId is required.");

			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.")
				.MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

			RuleFor(x => x.Description)
				.MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

			RuleFor(x => x.EstimateValue)
				.GreaterThanOrEqualTo(0).WithMessage("Estimate value must be greater than or equal to 0.");

			RuleFor(x => x.EstimateUnit)
				.GreaterThan(0).WithMessage("Estimate unit must be greater than 0.");

			RuleFor(x => x.DueDate)
				.GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.");

			RuleFor(x => x.Amount)
				.GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0.");

			RuleFor(x => x.Currency)
				.NotEmpty().WithMessage("Currency is required.")
				.Length(3).WithMessage("Currency code must be exactly 3 characters.");
		}
	}
}
