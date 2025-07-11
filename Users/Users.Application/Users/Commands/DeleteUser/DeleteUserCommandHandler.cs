using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Commands.DeleteUser
{
	public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
	{
		private readonly IUserRepository _userRepo;

		public DeleteUserCommandHandler(IUserRepository userRepo) => _userRepo = userRepo;

		public async Task<Result> Handle(DeleteUserCommand cmd, CancellationToken ct)
		{
			var user = await _userRepo.GetById(cmd.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			if (cmd.HardDelete)
			{
				await _userRepo.Delete(cmd.UserId, ct);
				return Result.Ok();
			}

			var result = user.Revoke(cmd.RevokedBy);
			if (result.IsFailed)
				return result;

			await _userRepo.Update(user, ct);
			return Result.Ok();
		}
	}
}
