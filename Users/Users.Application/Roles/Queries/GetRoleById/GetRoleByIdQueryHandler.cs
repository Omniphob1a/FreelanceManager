using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Users.Application.DTOs;
using Users.Application.Roles.Queries.GetRoleById;
using Users.Domain.Interfaces.Repositories;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
	private readonly IRoleRepository _repository;
	private readonly IMapper _mapper;
	private readonly ILogger<GetRoleByIdQueryHandler> _logger;

	public GetRoleByIdQueryHandler(IRoleRepository repository, IMapper mapper, ILogger<GetRoleByIdQueryHandler> logger)
	{
		_repository = repository;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken ct)
	{
		var role = await _repository.GetById(request.RoleId, ct);
		if (role is null)
		{
			_logger.LogWarning("Role {RoleId} not found", request.RoleId);
			return Result.Fail<RoleDto>("Role not found");
		}

		var dto = _mapper.Map<RoleDto>(role);
		return Result.Ok(dto);
	}
}