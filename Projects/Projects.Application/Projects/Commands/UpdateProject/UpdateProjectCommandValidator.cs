using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.UpdateProject
{
	public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
	{
		public UpdateProjectCommandValidator() 
		{
			const string titlePattern = "^[A-Za-z0-9 ]+$";

			RuleFor(x => x.ProjectId).NotEmpty();
			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.")
				.Length(3, 40).WithMessage("Title must be between 3 and 40 characters.")
				.Matches(titlePattern).WithMessage("Title can only contain letters, numbers, and spaces.");
			RuleFor(x => x.Description).MaximumLength(1000);
			RuleFor(x => x.BudgetMin).GreaterThanOrEqualTo(0);
			RuleFor(x => x.CurrencyCode).NotEmpty();
			RuleFor(x => x.Tags).NotNull();
		}
	}
}
