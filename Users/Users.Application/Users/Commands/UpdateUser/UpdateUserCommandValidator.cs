using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Users.Commands.UpdateUser;

namespace Users.Application.Users.Commands.UpdateUser
{
	public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
	{
		public UpdateUserCommandValidator()
		{
			const string namePattern = "^[A-Za-zА-Яа-я]+$";

			RuleFor(x => x.NewName)
				.NotEmpty().WithMessage("Name is required.")
				.Matches(namePattern).WithMessage("Name can only contain letters.");

			RuleFor(x => x.NewGender)
				.InclusiveBetween(0, 2).WithMessage("Gender must be 0 (Female), 1 (Male) or 2 (Unknown)");

			RuleFor(x => x.NewBirthday)
				.LessThanOrEqualTo(DateTime.UtcNow).When(x => x.NewBirthday.HasValue)
				.WithMessage("Birthday cannot be in the future.");

			RuleFor(x => x.ModifiedBy)
				.NotEmpty().WithMessage("ModifiedBy is required.");
		}
	}

}
