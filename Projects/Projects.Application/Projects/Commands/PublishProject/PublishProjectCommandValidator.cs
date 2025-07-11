using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.PublishProject
{
	public class PublishProjectCommandValidator : AbstractValidator<PublishProjectCommand>
	{
		public PublishProjectCommandValidator() 
		{
			RuleFor(x => x.ProjectId)
				.NotEmpty();
			RuleFor(x => x.ExpiresAt)
				.Must(x => x > DateTime.UtcNow).WithMessage("Expiration date must be in the future.");
		}	
	}
}
