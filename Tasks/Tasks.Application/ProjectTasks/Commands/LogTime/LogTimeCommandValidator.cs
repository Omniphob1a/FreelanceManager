using FluentValidation;

namespace Tasks.Application.ProjectTasks.Commands.AddTimeEntry
{
	public class LogTimeCommandValidator : AbstractValidator<LogTimeCommand>
	{
		public LogTimeCommandValidator()
		{
			RuleFor(x => x.TaskId)
				.NotEmpty().WithMessage("TaskId is required.");

			RuleFor(x => x.UserId)
				.NotEmpty().WithMessage("UserId is required.");

			RuleFor(x => x.StartedAt)
				.LessThan(x => x.EndedAt)
				.WithMessage("StartedAt must be before EndedAt.");

			RuleFor(x => x.EndedAt)
				.GreaterThan(x => x.StartedAt)
				.WithMessage("EndedAt must be after StartedAt.");

			RuleFor(x => x.Description)
				.MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

			When(x => x.HourlyRate.HasValue, () =>
			{
				RuleFor(x => x.HourlyRate.Value)
					.GreaterThanOrEqualTo(0).WithMessage("Hourly rate must be greater than or equal to 0.");

				RuleFor(x => x.Currency)
					.NotEmpty().WithMessage("Currency is required when HourlyRate is specified.")
					.Length(3).WithMessage("Currency code must be exactly 3 characters.");
			});
		}
	}
}
