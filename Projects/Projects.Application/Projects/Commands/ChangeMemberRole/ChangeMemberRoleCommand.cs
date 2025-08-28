using FluentResults;
using MediatR;
using Projects.Application.DTOs;

namespace Projects.Application.Projects.Commands.ChangeMemberRole;

public record ChangeMemberRoleCommand(
	Guid ProjectId,
	Guid UserId,
	string NewRole
) : IRequest<Result<ProjectMemberDto>>;
