﻿using FluentResults;
using MediatR;
using Users.Application.Interfaces;
using Users.Application.Responses;
using Users.Application.Users.Queries.AuthenticateUser;
using Users.Domain.Interfaces.Repositories;

public class AuthenticateUserQueryHandler
	: IRequestHandler<AuthenticateUserQuery, Result<AuthenticationResult>>
{
	private readonly IUserRepository _userRepo;
	private readonly IJwtTokenGenerator _jwtGen;

	public AuthenticateUserQueryHandler(
		IUserRepository userRepo,
		IJwtTokenGenerator jwtGen)
	{
		_userRepo = userRepo;
		_jwtGen = jwtGen;
	}

	public async Task<Result<AuthenticationResult>> Handle(
		AuthenticateUserQuery cmd,
		CancellationToken ct)
	{
		var user = await _userRepo.GetByLogin(cmd.Login, ct);
		if (user is null || !user.VerifyPassword(cmd.Password))
			return Result.Fail("Invalid credentials or user revoked");

		var roles = (await _userRepo.GetUserRoles(user.Id, ct))
						.Distinct()
						.ToList();

		var token = await _jwtGen.GenerateToken(
			user.Id,
			user.Login,
			roles
		);

		return Result.Ok(new AuthenticationResult
		{
			UserId = user.Id,
			Token = token,
			ExpiresAt = DateTime.UtcNow.AddMinutes(60)
		});
	}
}
