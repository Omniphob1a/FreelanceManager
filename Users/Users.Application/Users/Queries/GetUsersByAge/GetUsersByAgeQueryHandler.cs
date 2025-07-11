using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Users.Queries.GetUsersByAge
{
	public class GetUsersByAgeQueryHandler : IRequestHandler<GetUsersByAgeQuery, IEnumerable<UserDto>>
	{
		private readonly IUserRepository _userRepo;
		private readonly IRoleRepository _roleRepo;
		private readonly IPermissionRepository _permRepo;

		public GetUsersByAgeQueryHandler(
			IUserRepository userRepo,
			IRoleRepository roleRepo,
			IPermissionRepository permRepo)
		{
			_userRepo = userRepo;
			_roleRepo = roleRepo;
			_permRepo = permRepo;
		}

		public async Task<IEnumerable<UserDto>> Handle(GetUsersByAgeQuery request, CancellationToken ct)
		{
			var users = await _userRepo.ListByAge(request.MinAge, ct);
			var list = new List<UserDto>();

			foreach (var u in users)
			{
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

				list.Add(new UserDto
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
				});
			}

			return list;
		}
	}
}
