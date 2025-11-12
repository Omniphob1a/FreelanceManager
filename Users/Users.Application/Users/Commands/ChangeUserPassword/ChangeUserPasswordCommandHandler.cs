using FluentResults;
using MapsterMapper;
using MediatR;
using System.Security.Cryptography;
using System.Text;
using Users.Application.Contracts;
using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Commands.ChangeUserPassword
{
	public class ChangeUserPasswordCommandHandler
		: IRequestHandler<ChangeUserPasswordRequest, Result>
	{
		private readonly IUserRepository _userRepo;
		private readonly IOutboxService _outboxService;
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public ChangeUserPasswordCommandHandler(IUserRepository userRepo, IOutboxService outboxService, IMapper mapper, IUnitOfWork unitOfWork)
		{
			_userRepo = userRepo;
			_outboxService = outboxService;
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}
			
		public async Task<Result> Handle(ChangeUserPasswordRequest request, CancellationToken ct)
		{
			var user = await _userRepo.GetById(request.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			try
			{
				var hash = Convert.ToBase64String(
					SHA256.Create()
						  .ComputeHash(Encoding.UTF8.GetBytes(request.Command.NewPassword)));

				user.ChangePassword(hash, request.Command.ModifiedBy);

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
