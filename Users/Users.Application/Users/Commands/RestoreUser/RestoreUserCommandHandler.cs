﻿using FluentResults;
using MapsterMapper;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Commands.RestoreUser
{
	public class RestoreUserCommandHandler : IRequestHandler<RestoreUserCommand, Result>
	{
		private readonly IUserRepository _userRepo;
		private readonly IOutboxService _outboxService;
		private readonly IMapper _mapper;

		public RestoreUserCommandHandler(IUserRepository userRepo, IOutboxService outboxService, IMapper mapper)
		{
			_userRepo = userRepo;
			_outboxService = outboxService;
			_mapper = mapper;
		}

		public async Task<Result> Handle(RestoreUserCommand cmd, CancellationToken ct)
		{
			var user = await _userRepo.GetById(cmd.UserId, ct);
			if (user is null)
				return Result.Fail("User not found");

			try
			{
				user.Restore(cmd.ModifiedBy);

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
