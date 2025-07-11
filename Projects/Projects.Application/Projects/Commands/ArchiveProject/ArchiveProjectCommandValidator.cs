using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.ArchiveProject
{
	public class ArchiveProjectCommandValidator : AbstractValidator<ArchiveProjectCommand>
	{
		public ArchiveProjectCommandValidator() 
		{
			RuleFor(x => x.ProjectId).NotEmpty();
		}
	}
}
