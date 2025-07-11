using FluentResults;
using MediatR;
using System.Text;
using Users.Application.Contracts;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Commands.ChangeUserPassword
{
	public class ChangeUserPasswordCommandHandler
		: IRequestHandler<ChangeUserPasswordRequest, Result>
	{
		private readonly IUserRepository _userRepo;

		public ChangeUserPasswordCommandHandler(IUserRepository userRepo) => _userRepo = userRepo;

		public async Task<Result> Handle(
			ChangeUserPasswordRequest request,
			CancellationToken ct)
		{
			var user = await _userRepo.GetById(request.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			var result = user.UpdatePassword(
				request.Command.NewPassword,
				request.Command.ModifiedBy);

			if (result.IsFailed)
				return result;

			await _userRepo.Update(user, ct);
			return Result.Ok();
		}
	}
}