using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Responses;

namespace Users.Application.Users.Queries.AuthenticateUser
{
	public record AuthenticateUserQuery(string Login, string Password) : IRequest<Result<AuthenticationResult>>;
}
