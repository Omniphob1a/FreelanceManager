using FluentResults;
using MapsterMapper;
using MediatR;
using System.Security.Cryptography;
using System.Text;
using Users.Application.DTOs;
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
		private readonly IOutboxService _outboxService;
		private readonly IMapper _mapper;

		public RegisterUserCommandHandler(
			IUserRepository userRepo,
			IJwtTokenGenerator jwtGen,
			IRoleRepository roleRepo,
			IOutboxService outboxService,
			IMapper mapper)
		{
			_userRepo = userRepo;
			_jwtGen = jwtGen;
			_roleRepo = roleRepo;
			_outboxService = outboxService;
			_mapper = mapper;
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

			User user;
			try
			{
				user = User.Register(
					login: cmd.Login,
					passwordHash: hash,
					name: cmd.Name,
					gender: cmd.Gender,
					email: emailObj,
					createdBy: cmd.CreatedBy
				);

				if (birthdayUtc.HasValue)
				{
					user.UpdateProfile(cmd.Name, cmd.Gender, birthdayUtc, emailObj, cmd.CreatedBy);
				}

				if (cmd.IsAdmin)
				{
					user.AddRole(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), cmd.CreatedBy);
				}

				// базовая роль для всех
				user.AddRole(Guid.Parse("b9654606-8b85-4a67-a997-60128896fe4d"), cmd.CreatedBy);
			}
			catch (Exception ex)
			{
				return Result.Fail(new Error(ex.Message));
			}



			var dto = _mapper.Map<PublicUserDto>(user);
			var topic = "users";
			var key = user.Id.ToString();
			await _outboxService.Add(dto, topic, key, ct);

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
