using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.DeleteProjectTask
{
	public class DeleteProjectTaskCommandValidator : AbstractValidator<DeleteProjectTaskCommand>
	{
		public DeleteProjectTaskCommandValidator()
		{
			RuleFor(x => x.TaskId)
				.NotEmpty()
				.WithMessage("TaskId is required.");
		}
	}
}
