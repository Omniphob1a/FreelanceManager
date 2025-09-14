using FluentResults;
using MapsterMapper;
using MediatR;
using Users.Application.Contracts;
using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Domain.Interfaces.Repositories;
using Users.Domain.ValueObjects;

namespace Users.Application.Users.Commands.UpdateUser
{
	public class UpdateUserCommandHandler
		: IRequestHandler<UpdateUserRequest, Result>
	{
		private readonly IUserRepository _userRepo;
		private readonly IOutboxService _outboxService;
		private readonly IMapper _mapper;

		public UpdateUserCommandHandler(IUserRepository userRepo, IOutboxService outboxService, IMapper mapper)
		{
			_outboxService = outboxService;
			_userRepo = userRepo;
			_mapper = mapper;
		}

		public async Task<Result> Handle(UpdateUserRequest request, CancellationToken ct)
		{
			var user = await _userRepo.GetById(request.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			try
			{
				var emailObj = new Email(request.Command.NewEmail);

				user.UpdateProfile(
					request.Command.NewName,
					request.Command.NewGender,
					request.Command.NewBirthday,
					emailObj,
					request.Command.ModifiedBy
				);

				var dto = _mapper.Map<PublicUserDto>(user);
				var topic = "users";
				var key = user.Id.ToString();
				await _outboxService.Add(dto, topic, key, ct);

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
