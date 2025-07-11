using FluentResults;
using MediatR;
using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.AddMilestone
{
	public record AddMilestoneCommand(Guid ProjectId, string Title, DateTime DueDate) : IRequest<Result<ProjectMilestoneDto>>;
}
