using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.RegisterUser
{
	public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
	{
		public RegisterUserCommandValidator()
		{
			const string loginPattern = "^[A-Za-z0-9]+$";
			const string namePattern = "^[A-Za-zА-Яа-я]+$";

			RuleFor(x => x.Login)
				.NotEmpty().WithMessage("Login is required.")
				.Matches(loginPattern).WithMessage("Login can only contain letters and digits.");

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required.")
				.Matches(loginPattern).WithMessage("Password can only contain letters and digits.");

			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required.")
				.Matches(namePattern).WithMessage("Name can only contain letters.");

			RuleFor(x => x.Gender)
				.InclusiveBetween(0, 2).WithMessage("Gender must be 0 (Female), 1 (Male) or 2 (Unknown)");

			RuleFor(x => x.Birthday)
				.LessThanOrEqualTo(DateTime.UtcNow).When(x => x.Birthday.HasValue)
				.WithMessage("Birthday cannot be in the future.");

			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");

			RuleFor(x => x.CreatedBy)
				.NotEmpty().WithMessage("CreatedBy is required.");
		}
	}

}
