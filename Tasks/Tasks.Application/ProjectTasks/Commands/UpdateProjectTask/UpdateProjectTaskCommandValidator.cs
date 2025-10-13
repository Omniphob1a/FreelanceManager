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

			RuleFor(x => x.TimeEstimated)
				.Must(te => te == null || te.Value.TotalMinutes >= 0)
				.WithMessage("Time estimated must be greater than or equal to 0 minutes.");

			RuleFor(x => x.DueDate)
				.Must(d => d == null || d > DateTime.UtcNow)
				.WithMessage("Due date must be in the future.");

			RuleFor(x => x.Priority)
				.InclusiveBetween(0, 2) 
				.WithMessage("Priority must be between 0 and 2.");
		}
	}

}
