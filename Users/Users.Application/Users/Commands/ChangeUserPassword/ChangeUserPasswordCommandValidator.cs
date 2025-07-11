using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.ChangeUserPassword
{
	public class ChangePasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
	{
		public ChangePasswordCommandValidator()
		{
			const string pwdPattern = "^[A-Za-z0-9]+$";

			RuleFor(x => x.NewPassword)
				.NotEmpty().WithMessage("NewPassword is required.")
				.Matches(pwdPattern).WithMessage("NewPassword can only contain letters and digits.");

			RuleFor(x => x.ModifiedBy)
				.NotEmpty().WithMessage("ModifiedBy is required.");
		}
	}

}
