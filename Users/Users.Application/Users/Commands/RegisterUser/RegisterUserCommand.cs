using FluentResults;
using MediatR;
using Users.Application.Responses;

namespace Users.Application.Users.Commands.RegisterUser
{
	public record RegisterUserCommand(
	string Login,
	string Password,
	string Name,
	int Gender,
	DateTime? Birthday,
	string Email,
	bool IsAdmin,
	string CreatedBy
) : IRequest<Result<AuthenticationResult>>;
}
