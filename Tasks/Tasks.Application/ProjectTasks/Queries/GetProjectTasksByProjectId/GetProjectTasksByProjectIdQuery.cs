using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Filters;
using Tasks.Application.Common.Pagination;
using Tasks.Application.DTOs;

namespace Tasks.Application.ProjectTasks.Queries.GetProjectTasksByProjectId
{
	public record GetProjectTasksByProjectIdQuery(Guid ProjectId, TaskFilter filter, PaginationInfo paginationInfo) : IRequest<Result<PaginatedResult<ProjectTaskDto>>>;
}
