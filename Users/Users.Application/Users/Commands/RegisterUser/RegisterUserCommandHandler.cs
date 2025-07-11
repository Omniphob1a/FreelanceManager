using FluentResults;
using MediatR;
using System.Security.Cryptography;
using System.Text;
using Users.Application.Interfaces;
using Users.Application.Responses;
using Users.Domain.Entities;
using Users.Domain.Interfaces.Repositories;
using Users.Domain.ValueObjects;

namespace Users.Application.Users.Commands.RegisterUser
{
	public class RegisterUserCommandHandler
		: IRequestHandler<RegisterUserCommand, Result<AuthenticationResult>>
	{
		private readonly IUserRepository _userRepo;
		private readonly IRoleRepository _roleRepo;
		private readonly IJwtTokenGenerator _jwtGen;

		public RegisterUserCommandHandler(
			IUserRepository userRepo,
			IJwtTokenGenerator jwtGen,
			IRoleRepository roleRepo)
		{
			_userRepo = userRepo;
			_jwtGen = jwtGen;
			_roleRepo = roleRepo;
		}

		public async Task<Result<AuthenticationResult>> Handle(
			RegisterUserCommand cmd,
			CancellationToken ct)
		{
			var hash = Convert.ToBase64String(
				SHA256.Create()
					  .ComputeHash(Encoding.UTF8.GetBytes(cmd.Password)));

			DateTime? birthdayUtc = cmd.Birthday.HasValue
				? DateTime.SpecifyKind(cmd.Birthday.Value, DateTimeKind.Utc)
				: (DateTime?)null;

			var emailObj = new Email(cmd.Email);

			var userResult = User.TryCreate(
				cmd.Login,
				hash,
				cmd.Name,
				cmd.Gender,
				birthdayUtc,
				emailObj,
				cmd.CreatedBy,
				cmd.IsAdmin);

			if (userResult.IsFailed)
				return Result.Fail(userResult.Errors);

			var user = userResult.Value;

			if (user.Admin)
			{
				var adminRoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
				user.RoleIds.Add(adminRoleId);
			}

			await _userRepo.Add(user, ct);

			var roleNames = await _roleRepo.GetRoleNamesByIds(user.RoleIds, ct);

			var token = await _jwtGen.GenerateToken(
				user.Id,
				user.Login,
				roleNames);

			return Result.Ok(new AuthenticationResult
			{
				UserId = user.Id,
				Token = token,
				ExpiresAt = DateTime.UtcNow.AddMinutes(60)
			});
		}
	}
}

