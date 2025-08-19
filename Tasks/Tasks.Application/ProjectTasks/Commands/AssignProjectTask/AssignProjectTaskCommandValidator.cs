using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.AssignProjectTask
{
	public class AssignProjectTaskCommandValidator : AbstractValidator<AssignProjectTaskCommand>
	{
		public AssignProjectTaskCommandValidator() 
		{ 
			RuleFor(x => x.TaskId)
				.NotEmpty().WithMessage("Task ID is required.")
				.Must(x => x != Guid.Empty).WithMessage("Task ID cannot be empty.");
			RuleFor(x => x.AssigneeId)
				.NotEmpty().WithMessage("Assignee ID is required.")
				.Must(x => x != Guid.Empty).WithMessage("Assignee ID cannot be empty.");
		}	
	}
}
