using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.CancelProjectTask
{
	public record CancelProjectTaskCommand(Guid TaskId, string Reason) : IRequest<Result<Guid>>;
}
