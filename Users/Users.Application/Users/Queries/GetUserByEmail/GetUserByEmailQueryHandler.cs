using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Queries.GetUserByEmail
{
	public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto>
	{
		private readonly IUserRepository _userRepo;
		private readonly IRoleRepository _roleRepo;
		private readonly IPermissionRepository _permRepo;

		public GetUserByEmailQueryHandler(IUserRepository userRepo, IRoleRepository roleRepo, IPermissionRepository permRepo)
		{
			_userRepo = userRepo;
			_roleRepo = roleRepo;
			_permRepo = permRepo;
		}

		public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken ct)
		{
			var u = await _userRepo.GetByEmail(request.Email, ct)
				?? throw new KeyNotFoundException("User not found");

			var roleNames = new List<string>();
			var permIds = new HashSet<Guid>();

			foreach (var rid in u.RoleIds)
			{
				var role = await _roleRepo.GetById(rid, ct);
				if (role is null) continue;
				roleNames.Add(role.Name);
				foreach (var pid in role.PermissionIds)
					permIds.Add(pid);
			}

			var permNames = new List<string>();
			foreach (var pid in permIds)
			{
				var p = await _permRepo.GetById(pid, ct);
				if (p != null)
					permNames.Add(p.Name);
			}

			return new UserDto
			{
				Id = u.Id,
				Login = u.Login,
				Name = u.Name,
				Gender = u.Gender,
				Birthday = u.Birthday,
				Email = u.Email.Value,
				Admin = u.Admin,
				CreatedAt = u.CreatedAt,
				ModifiedOn = u.ModifiedOn,
				RevokedOn = u.RevokedOn,
				Roles = roleNames,
				Permissions = permNames
			};
		}
	}
}
