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
		private readonly IUnitOfWork _unitOfWork;

		public DeleteUserCommandHandler(IUserRepository userRepo, IOutboxService outboxService, IUnitOfWork unitOfWork)
		{
			_userRepo = userRepo;
			_outboxService = outboxService;
			_unitOfWork = unitOfWork;
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


				await _userRepo.Update(user, ct);
				_unitOfWork.TrackEntity(user);
				await _unitOfWork.SaveChangesAsync();
				return Result.Ok();
			}
			catch (Exception ex)
			{
				return Result.Fail(new Error(ex.Message));
			}
		}
	}
}
