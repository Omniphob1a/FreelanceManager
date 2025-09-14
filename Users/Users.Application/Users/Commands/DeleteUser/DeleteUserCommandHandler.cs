using FluentResults;
using MediatR;
using Users.Application.Interfaces;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Commands.DeleteUser
{
	public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
	{
		private readonly IUserRepository _userRepo;
		private readonly IOutboxService _outboxService;

		public DeleteUserCommandHandler(IUserRepository userRepo, IOutboxService outboxService)
		{
			_userRepo = userRepo;
			_outboxService = outboxService;
		}
	

		public async Task<Result> Handle(DeleteUserCommand cmd, CancellationToken ct)
		{
			var user = await _userRepo.GetById(cmd.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			if (cmd.HardDelete)
			{
				// Полное удаление
				await _userRepo.Delete(cmd.UserId, ct);
				return Result.Ok();
			}

			try
			{
				// Мягкое удаление 
				user.Delete(cmd.RevokedBy);

				string topic = "users"; 
				string key = user.Id.ToString();
				await _outboxService.AddTombstone(topic, key, ct);

				await _userRepo.Update(user, ct);
				return Result.Ok();
			}
			catch (Exception ex)
			{
				return Result.Fail(new Error(ex.Message));
			}
		}
	}
}
