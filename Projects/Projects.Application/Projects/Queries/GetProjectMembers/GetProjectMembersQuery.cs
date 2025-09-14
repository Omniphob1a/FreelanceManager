using FluentResults;
using MediatR;
using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetProjectMembers
{
	public record GetProjectMembersQuery(Guid ProjectId) : IRequest<Result<List<ProjectMemberReadDto>>>;
}
