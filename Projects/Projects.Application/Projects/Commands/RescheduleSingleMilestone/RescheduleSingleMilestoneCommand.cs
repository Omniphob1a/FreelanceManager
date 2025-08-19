using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.RescheduleMilestone
{
	public record RescheduleSingleMilestoneCommand(Guid ProjectId, Guid MilestoneId, DateTime NewDueDate) : IRequest<Result>;
}
