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
		private readonly IUnitOfWork _unitOfWork;

		public RegisterUserCommandHandler(
			IUserRepository userRepo,
			IJwtTokenGenerator jwtGen,
			IRoleRepository roleRepo,
			IOutboxService outboxService,
			IMapper mapper,
			IUnitOfWork unitOfWork)
		{
			_userRepo = userRepo;
			_jwtGen = jwtGen;
			_roleRepo = roleRepo;
			_outboxService = outboxService;
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Result<AuthenticationResult>> Handle(
			RegisterUserCommand cmd,
			CancellationToken ct)
		{
			var hash = Convert.ToBase64String(
				SHA256.Create()
					  .ComputeHash(Encoding.UTF8.GetBytes(cmd.Password)));

			DateTime birthdayUtc = DateTime.SpecifyKind(cmd.Birthday, DateTimeKind.Utc);

			var emailObj = new Email(cmd.Email);

			User user;
			try
			{
				user = User.Register(
					login: cmd.Login,
					passwordHash: hash,
					name: cmd.Name,
					gender: cmd.Gender,
					birthday: birthdayUtc,
					email: emailObj,
					createdBy: cmd.CreatedBy
				);

				if (cmd.IsAdmin)
				{
					user.AddRole(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), cmd.CreatedBy);
				}

				// базовая роль для всех
				user.AddRole(Guid.Parse("5eb8aa01-9c0f-412b-97fa-64c0de9a67d6"), cmd.CreatedBy);
			}
			catch (Exception ex)
			{
				return Result.Fail(new Error(ex.Message));
			}

			await _userRepo.Add(user, ct);

			var roleNames = await _roleRepo.GetRoleNamesByIds(user.RoleIds, ct);

			var token = await _jwtGen.GenerateToken(
				user.Id,
				user.Login,
				roleNames);

			_unitOfWork.TrackEntity(user);
			await _unitOfWork.SaveChangesAsync();


			return Result.Ok(new AuthenticationResult
			{
				UserId = user.Id,
				Token = token,
				ExpiresAt = DateTime.UtcNow.AddMinutes(60)
			});
		}
	}
}
