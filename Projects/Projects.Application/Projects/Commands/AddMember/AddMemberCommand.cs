using FluentResults;
using MediatR;
using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.AddMember
{
	public record AddMemberCommand(
		Guid ProjectId,
		string Email,
		string Role
	) : IRequest<Result<ProjectMemberDto>>;
}
