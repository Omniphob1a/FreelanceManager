using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Commands.RestoreUser
{
	public class RestoreUserCommandHandler : IRequestHandler<RestoreUserCommand, Result>
	{
		private readonly IUserRepository _userRepo;

		public RestoreUserCommandHandler(IUserRepository userRepo) => _userRepo = userRepo;

		public async Task<Result> Handle(RestoreUserCommand cmd, CancellationToken ct)
		{
			var user = await _userRepo.GetById(cmd.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			user.UpdateUser(user.Name, user.Gender, user.Birthday, user.Email, cmd.ModifiedBy);
			var result = user.Restore(cmd.ModifiedBy);

			if (result.IsFailed)
				return result;

			await _userRepo.Update(user, ct);
			return Result.Ok();
		}
	}
}
