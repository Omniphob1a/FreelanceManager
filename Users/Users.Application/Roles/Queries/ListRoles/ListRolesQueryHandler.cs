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

namespace Users.Application.Roles.Queries.ListRoles
{
	public class ListRolesQueryHandler : IRequestHandler<ListRolesQuery, Result<IReadOnlyList<RoleDto>>>
	{
		private readonly IRoleRepository _repository;
		private readonly IMapper _mapper;
		private readonly ILogger<ListRolesQueryHandler> _logger;

		public ListRolesQueryHandler(IRoleRepository repository, IMapper mapper, ILogger<ListRolesQueryHandler> logger)
		{
			_repository = repository;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<Result<IReadOnlyList<RoleDto>>> Handle(ListRolesQuery request, CancellationToken ct)
		{
			var roles = await _repository.ListAll(ct);
			var dtos = roles.Select(r => _mapper.Map<RoleDto>(r)).ToList().AsReadOnly();
			return Result.Ok<IReadOnlyList<RoleDto>>(dtos);
		}
	}
}
