using FluentResults;
using MediatR;

namespace Users.Application.Projects.Commands.ConfirmProject
{
	public record ConfirmProjectCommand(Guid ProjectId, Guid UserId) : IRequest<Result<Unit>>;
}
