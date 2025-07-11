using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.CreateProject
{
	public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
	{
		public CreateProjectCommandValidator() 
		{
			const string titlePattern = "^[A-Za-z0-9 ]+$";
			const string currencyCodePattern = "^[A-Z]{3}$";

			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.")
				.Length(3, 40).WithMessage("Title must be between 3 and 40 characters.")
				.Matches(titlePattern).WithMessage("Title can only contain letters, numbers, and spaces.");
			RuleFor(x => x.Description).MaximumLength(1000);
			RuleFor(x => x.BudgetMin).GreaterThanOrEqualTo(0);
			RuleFor(x => x.CurrencyCode)
				.NotEmpty().WithMessage("CurrencyCode is required")
				.Length(3).WithMessage("CurrencyCode must be 3 characters")
				.Matches(currencyCodePattern).WithMessage("Currency code can only contain 3 letters");
			RuleFor(x => x.Tags).NotNull();
		}
	}
}
