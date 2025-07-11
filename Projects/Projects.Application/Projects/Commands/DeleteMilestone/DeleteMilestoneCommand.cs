using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.DeleteAttachment
{
	public record DeleteMilestoneCommand(Guid ProjectId, Guid MilestoneId) : IRequest<Result>;
}
