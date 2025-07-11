using FluentResults;
using MediatR;
using Users.Application.Users.Commands.ChangeUserLogin;

namespace Users.Application.Contracts
{
	public record ChangeUserLoginRequest(
		Guid UserId,
		ChangeUserLoginCommand Command
	) : IRequest<Result>;
}
