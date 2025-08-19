using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.StartProjectTask
{
	public record StartProjectTaskCommand(Guid TaskId) : IRequest<Result<Unit>>;
}
