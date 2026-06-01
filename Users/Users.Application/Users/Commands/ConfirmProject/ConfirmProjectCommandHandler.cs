using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Users.Application.Interfaces;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Projects.Commands.ConfirmProject
{
	public class ConfirmProjectCommandHandler : IRequestHandler<ConfirmProjectCommand, Result<Unit>>
	{
		private readonly IUserRepository _userRepo;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<ConfirmProjectCommandHandler> _logger;

		public ConfirmProjectCommandHandler(
			IUserRepository userRepo,
			IUnitOfWork unitOfWork,
			ILogger<ConfirmProjectCommandHandler> logger)
		{
			_userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<Result<Unit>> Handle(ConfirmProjectCommand cmd, CancellationToken ct)
		{
			try
			{
				_logger.LogInformation("ConfirmProjectCommand start: ProjectId={ProjectId}, UserId={UserId}", cmd.ProjectId, cmd.UserId);

				var user = await _userRepo.GetById(cmd.UserId, ct);
				if (user == null)
				{
					_logger.LogWarning("User not found: {UserId}", cmd.UserId);
					return Result.Fail<Unit>($"User {cmd.UserId} not found");
				}

				user.ConfirmProject(cmd.ProjectId);

				await _userRepo.Update(user, ct);

				_unitOfWork.TrackEntity(user);
				await _unitOfWork.SaveChangesAsync(ct);

				_logger.LogInformation("ConfirmProjectCommand done: ProjectId={ProjectId}, UserId={UserId}, RegisteredObjects={Count}",
					cmd.ProjectId, cmd.UserId, user.RegisteredObjects);

				return Result.Ok(Unit.Value);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error handling ConfirmProjectCommand ProjectId={ProjectId} UserId={UserId}", cmd.ProjectId, cmd.UserId);
				return Result.Fail<Unit>(ex.Message);
			}
		}
	}
}
