using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.CompleteProject
{
	public class CompleteProjectCommandValidator : AbstractValidator<CompleteProjectCommand>
	{
		public CompleteProjectCommandValidator() 
		{
			RuleFor(x => x.ProjectId).NotEmpty();
		}
	}
}
