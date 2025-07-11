using FluentResults;
using MediatR;
using Users.Application.Contracts;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Commands.ChangeUserLogin
{
	public class ChangeUserLoginCommandHandler
		: IRequestHandler<ChangeUserLoginRequest, Result>
	{
		private readonly IUserRepository _userRepo;

		public ChangeUserLoginCommandHandler(IUserRepository userRepo) => _userRepo = userRepo;

		public async Task<Result> Handle(
			ChangeUserLoginRequest request,
			CancellationToken ct)
		{
			var user = await _userRepo.GetById(request.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			var result = user.UpdateLogin(
				request.Command.NewLogin,
				request.Command.ModifiedBy);

			if (result.IsFailed)
				return result;

			await _userRepo.Update(user, ct);
			return Result.Ok();
		}
	}
}