using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.DeleteTags
{
	public class DeleteTagsCommandValidator : AbstractValidator<DeleteTagsCommand>
	{
		public DeleteTagsCommandValidator()
		{
			RuleFor(x => x.ProjectId)
				.NotEmpty().WithMessage("ProjectId must not be empty.");

			RuleFor(x => x.Tags)
				.NotNull().WithMessage("Tags list must not be null.")
				.NotEmpty().WithMessage("Tags list must not be empty.");

			RuleForEach(x => x.Tags)
				.Must(tag => !string.IsNullOrWhiteSpace(tag))
				.WithMessage("Tag must not contain only whitespace.");
		}
	}
}
