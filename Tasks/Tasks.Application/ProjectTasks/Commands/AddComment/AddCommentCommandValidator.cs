using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.AddComment
{
	public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
	{
		public AddCommentCommandValidator()
		{
			RuleFor(x => x.TaskId).NotEmpty().WithMessage("TaskId is required.");
			RuleFor(x => x.AuthorId).NotEmpty().WithMessage("AuthorId is required.");
			RuleFor(x => x.Text)
				.NotEmpty().WithMessage("Text is required.")
				.MaximumLength(2000).WithMessage("Text must not exceed 2000 characters.")
				.Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Text cannot be whitespace.");
		}
	}
}
