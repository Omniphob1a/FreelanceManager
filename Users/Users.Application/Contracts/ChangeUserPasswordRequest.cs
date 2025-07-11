using FluentResults;
using MediatR;
using Users.Application.Users.Commands.ChangeUserPassword;

namespace Users.Application.Contracts
{
	public record ChangeUserPasswordRequest(
		Guid UserId,
		ChangeUserPasswordCommand Command
	) : IRequest<Result>;
}
