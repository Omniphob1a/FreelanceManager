using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Queries.GetUserByCredentials
{
	public class GetUserByCredentialsQueryHandler : IRequestHandler<GetUserByCredentialsQuery, Result<UserDto>>
	{
		private readonly IUserRepository _userRepo;
		private readonly IRoleRepository _roleRepo;
		private readonly IPermissionRepository _permissionRepo;
		
		public GetUserByCredentialsQueryHandler(
			IUserRepository userRepo,
			IRoleRepository roleRepo,
			IPermissionRepository permissionRepo)
		{
			_userRepo = userRepo;
			_roleRepo = roleRepo;
			_permissionRepo = permissionRepo;
		}

		public async Task<Result<UserDto>> Handle(GetUserByCredentialsQuery query, CancellationToken ct)
		{
			var hashedPassword = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(query.Password)));

			var user = await _userRepo.GetByLogin(query.Login, ct);

			if (user is null || user.RevokedOn.HasValue || user.PasswordHash != hashedPassword)
				return Result.Fail("Invalid credentials or user is revoked");

			var roleNames = await _roleRepo.GetRoleNamesByIds(user.RoleIds, ct);
			var permissionNames = await _permissionRepo.GetPermissionsByRoleIds(user.RoleIds, ct);

			var userDto = new UserDto
			{
				Id = user.Id,
				Login = user.Login,
				Name = user.Name,
				Gender = user.Gender,
				Birthday = user.Birthday,
				Email = user.Email.Value,
				Admin = user.Admin,
				CreatedAt = user.CreatedAt,
				ModifiedOn = user.ModifiedOn,
				RevokedOn = user.RevokedOn,
				Roles = roleNames,
				Permissions = permissionNames
			};

			return Result.Ok(userDto);
		}
	}
}
