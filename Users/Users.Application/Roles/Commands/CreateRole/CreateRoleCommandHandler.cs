using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Users.Application.DTOs;
using Users.Application.Roles.Commands.CreateRole;
using Users.Domain.Entities;
using Users.Domain.Interfaces.Repositories;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
	private readonly IRoleRepository _repository;
	private readonly IMapper _mapper;
	private readonly ILogger<CreateRoleCommandHandler> _logger;

	public CreateRoleCommandHandler(IRoleRepository repository, IMapper mapper, ILogger<CreateRoleCommandHandler> logger)
	{
		_repository = repository;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Creating role with name {RoleName}", request.Name);

		var existing = await _repository.GetByName(request.Name, ct);
		if (existing is not null)
		{
			_logger.LogWarning("Role with name {RoleName} already exists", request.Name);
			return Result.Fail<RoleDto>("Role with given name already exists");
		}

		var tryCreate = Role.TryCreate(request.Name);
		if (tryCreate.IsFailed)
		{
			_logger.LogWarning("Validation failed when creating role {RoleName}: {Errors}", request.Name,
				string.Join(", ", tryCreate.Errors.Select(e => e.Message)));
			return Result.Fail<RoleDto>(tryCreate.Errors);
		}

		var role = tryCreate.Value;

		if (request.PermissionIds != null)
		{
			foreach (var pid in request.PermissionIds.Distinct())
				role.AddPermission(pid);
		}

		try
		{
			await _repository.Add(role, ct);
			var dto = _mapper.Map<RoleDto>(role);
			_logger.LogInformation("Role {RoleId} created", role.Id);
			return Result.Ok(dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create role {RoleName}", request.Name);
			return Result.Fail<RoleDto>("Unexpected error occurred while creating role");
		}
	}
}