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

namespace Users.Application.Roles.Commands.AddPermissionToRole
{
	public class AddPermissionToRoleCommandHandler : IRequestHandler<AddPermissionToRoleCommand, Result<RoleDto>>
	{
		private readonly IRoleRepository _repository;
		private readonly IMapper _mapper;
		private readonly ILogger<AddPermissionToRoleCommandHandler> _logger;

		public AddPermissionToRoleCommandHandler(IRoleRepository repository, IMapper mapper, ILogger<AddPermissionToRoleCommandHandler> logger)
		{
			_repository = repository;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<Result<RoleDto>> Handle(AddPermissionToRoleCommand request, CancellationToken ct)
		{
			_logger.LogDebug("Adding permission {PermissionId} to role {RoleId}", request.PermissionId, request.RoleId);

			var role = await _repository.GetById(request.RoleId, ct);
			if (role is null)
			{
				_logger.LogWarning("Role {RoleId} not found", request.RoleId);
				return Result.Fail<RoleDto>("Role not found");
			}

			role.AddPermission(request.PermissionId);

			try
			{
				await _repository.Update(role, ct);
				var dto = _mapper.Map<RoleDto>(role);
				_logger.LogInformation("Permission {PermissionId} added to role {RoleId}", request.PermissionId, request.RoleId);
				return Result.Ok(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to add permission to role {RoleId}", request.RoleId);
				return Result.Fail<RoleDto>("Unexpected error occurred while updating role");
			}
		}
	}
}
