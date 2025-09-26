using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;

namespace Tasks.Application.ProjectTasks.Queries.GetProjectMembers
{
	public record GetProjectMembersQuery(Guid ProjectId) : IRequest<Result<List<ProjectMemberReadDto>>>;
}
