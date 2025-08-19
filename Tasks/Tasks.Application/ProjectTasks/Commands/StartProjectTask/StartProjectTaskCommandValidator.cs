using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.StartProjectTask
{
	public class StartProjectTaskCommandValidator : AbstractValidator<StartProjectTaskCommand>
	{
		public StartProjectTaskCommandValidator()
		{
			RuleFor(x => x.TaskId)
				.NotEmpty().WithMessage("TaskId is required.")
				.Must(id => id != Guid.Empty).WithMessage("TaskId must be a valid GUID.");
		}
	}
}
