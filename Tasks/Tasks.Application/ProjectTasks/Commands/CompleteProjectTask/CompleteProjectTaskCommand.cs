using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.CompleteProjectTask
{
	public record CompleteProjectTaskCommand(Guid TaskId) : IRequest<Result<Unit>>;
}
