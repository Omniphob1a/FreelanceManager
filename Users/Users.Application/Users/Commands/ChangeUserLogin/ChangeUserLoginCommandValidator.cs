using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.ChangeUserLogin
{
	public class ChangeUserLoginCommandValidator : AbstractValidator<ChangeUserLoginCommand>
	{
		public ChangeUserLoginCommandValidator()
		{
			const string loginPattern = "^[A-Za-z0-9]+$";

			RuleFor(x => x.NewLogin)
				.NotEmpty().WithMessage("NewLogin is required.")
				.Matches(loginPattern).WithMessage("NewLogin can only contain letters and digits.");

			RuleFor(x => x.ModifiedBy)
				.NotEmpty().WithMessage("ModifiedBy is required.");
		}
	}
}
