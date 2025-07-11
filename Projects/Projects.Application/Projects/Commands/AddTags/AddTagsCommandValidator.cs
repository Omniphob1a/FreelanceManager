using FluentValidation;
using System;
using System.Collections.Generic;

namespace Projects.Application.Projects.Commands.AddTag
{
	public class AddTagsCommandValidator : AbstractValidator<AddTagsCommand>
	{
		public AddTagsCommandValidator()
		{
			RuleFor(x => x.Tags)
				.NotNull().WithMessage("Tags collection must not be null.")
				.NotEmpty().WithMessage("At least one tag must be provided.");

			RuleForEach(x => x.Tags)
				.Cascade(CascadeMode.Stop)
				.NotEmpty().WithMessage("Tag must not be empty or whitespace.")
				.MaximumLength(50).WithMessage("Tag length must not exceed 50 characters.")
				.Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("Tag can only contain letters, numbers, and hyphens.");
		}
	}
}
