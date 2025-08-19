using FluentValidation;

namespace Tasks.Application.ProjectTasks.Commands.CompleteProjectTask
{
    public class CompleteProjectTaskCommandValidator : AbstractValidator<CompleteProjectTaskCommand>
    {
        public CompleteProjectTaskCommandValidator()
        {
            RuleFor(x => x.TaskId)
                .NotEmpty()
                .WithMessage("TaskId is required.");
        }
    }
}
