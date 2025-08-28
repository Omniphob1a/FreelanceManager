using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common;
using Tasks.Application.DTOs;

namespace Tasks.Application.ProjectTasks.Queries.GetProjectTaskById
{
	public record GetProjectTaskByIdQuery(Guid TaskId, IEnumerable<TaskIncludeOptions> Includes) : IRequest<Result<ProjectTaskDto>>;
}
