using FluentValidation;
using Tasks.Application.ProjectTasks.Commands.UnassignProjectTask;

public class UnassignProjectTaskCommandValidator : AbstractValidator<UnassignProjectTaskCommand>
{
	public UnassignProjectTaskCommandValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty()
			.WithMessage("TaskId is required.");

		RuleFor(x => x.AssigneeId)
			.NotEmpty()
			.WithMessage("AssigneeId is required.");
	}
}
