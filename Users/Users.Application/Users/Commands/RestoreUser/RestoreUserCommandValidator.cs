using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.RestoreUser
{
	public class RestoreUserCommandValidator : AbstractValidator<RestoreUserCommand>
	{
		public RestoreUserCommandValidator()
		{
			RuleFor(x => x.UserId)
				.NotEmpty().WithMessage("UserId is required.");

			RuleFor(x => x.ModifiedBy)
				.NotEmpty().WithMessage("ModifiedBy is required.");
		}
	}

}
