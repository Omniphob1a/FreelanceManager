using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Roles.Commands.RemovePermissionFromRole
{
	public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, Result<RoleDto>>
	{
		private readonly IRoleRepository _repository;
		private readonly IMapper _mapper;
		private readonly ILogger<RemovePermissionFromRoleCommandHandler> _logger;

		public RemovePermissionFromRoleCommandHandler(IRoleRepository repository, IMapper mapper, ILogger<RemovePermissionFromRoleCommandHandler> logger)
		{
			_repository = repository;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<Result<RoleDto>> Handle(RemovePermissionFromRoleCommand request, CancellationToken ct)
		{
			_logger.LogDebug("Removing permission {PermissionId} from role {RoleId}", request.PermissionId, request.RoleId);

			var role = await _repository.GetById(request.RoleId, ct);
			if (role is null)
			{
				_logger.LogWarning("Role {RoleId} not found", request.RoleId);
				return Result.Fail<RoleDto>("Role not found");
			}

			role.RemovePermission(request.PermissionId);

			try
			{
				await _repository.Update(role, ct);
				var dto = _mapper.Map<RoleDto>(role);
				_logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", request.PermissionId, request.RoleId);
				return Result.Ok(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to remove permission from role {RoleId}", request.RoleId);
				return Result.Fail<RoleDto>("Unexpected error occurred while updating role");
			}
		}
	}
}
