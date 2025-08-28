using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Interfaces.Repositories;

namespace Users.Application.Roles.Commands.DeleteRole
{
	public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
	{
		private readonly IRoleRepository _repository;
		private readonly ILogger<DeleteRoleCommandHandler> _logger;

		public DeleteRoleCommandHandler(IRoleRepository repository, ILogger<DeleteRoleCommandHandler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
		{
			_logger.LogDebug("Deleting role {RoleId}", request.RoleId);

			var existing = await _repository.GetById(request.RoleId, ct);
			if (existing is null)
			{
				_logger.LogWarning("Role {RoleId} not found", request.RoleId);
				return Result.Fail("Role not found");
			}

			try
			{
				await _repository.Delete(request.RoleId, ct);
				_logger.LogInformation("Role {RoleId} deleted", request.RoleId);
				return Result.Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete role {RoleId}", request.RoleId);
				return Result.Fail("Unexpected error occurred while deleting role");
			}
		}
	}
}
