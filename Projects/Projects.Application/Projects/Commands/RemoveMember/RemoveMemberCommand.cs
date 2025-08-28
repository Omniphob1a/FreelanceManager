using FluentResults;
using MediatR;

namespace Projects.Application.Projects.Commands.RemoveMember;

public record RemoveMemberCommand(
	Guid ProjectId,
	string Email
) : IRequest<Result<Unit>>;
