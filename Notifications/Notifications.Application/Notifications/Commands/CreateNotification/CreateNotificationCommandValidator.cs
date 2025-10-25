using FluentValidation;
using Notifications.Application.Notifications.Commands.CreateNotification;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
	public CreateNotificationCommandValidator()
	{
		RuleFor(x => x.EventId)
			.NotEmpty()
			.WithMessage("EventId cannot be empty.");

		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("UserId cannot be empty.");

		RuleFor(x => x.Channel)
			.IsInEnum()
			.WithMessage("Invalid notification channel.");

		RuleFor(x => x.TemplateKey)
			.NotEmpty()
			.WithMessage("TemplateKey is required.")
			.MaximumLength(100)
			.WithMessage("TemplateKey must not exceed 100 characters.");

		RuleFor(x => x.Payload)
			.MaximumLength(5000)
			.WithMessage("Payload must not exceed 5000 characters.")
			.When(x => x.Payload != null);
	}
}
