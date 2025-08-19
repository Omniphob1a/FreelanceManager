using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.CreateProjectTask
{
	public class CreateProjectTaskCommandValidator : AbstractValidator<CreateProjectTaskCommand>
	{
		public CreateProjectTaskCommandValidator()
		{
			RuleFor(x => x.Title)
				.NotEmpty()
				.WithMessage("Task title is required.")
				.MaximumLength(100)
				.WithMessage("Task title must not exceed 100 characters.");
			RuleFor(x => x.Description)
				.MaximumLength(500)
				.WithMessage("Description must not exceed 500 characters.");
			RuleFor(x => x.ProjectId)
				.NotEmpty()
				.WithMessage("Project ID is required.");
		}
	}
}
