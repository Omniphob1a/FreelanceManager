using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.AddMilestone
{
	public class AddMilestoneCommandValidator : AbstractValidator<AddMilestoneCommand>
	{
		public AddMilestoneCommandValidator() 
		{
			const string titlePattern = "^[A-Za-z0-9 ]+$";
			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.")
				.Length(3, 40).WithMessage("Title must be between 3 and 40 characters.")
				.Matches(titlePattern).WithMessage("Title can only contain letters, numbers, and spaces.");

			RuleFor(x => x.ProjectId)
				.NotEmpty().WithMessage("ProjectId is required.");

			RuleFor(x => x.DueDate)
				.NotEmpty().WithMessage("Due date is required.")
				.Must(date => date > DateTime.UtcNow).WithMessage("Due date must be in the future.");
		}
	}
}
