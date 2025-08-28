using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Users.Application.DTOs;
using Users.Application.Roles.Commands.AddPermissionToRole;
using Users.Application.Roles.Commands.CreateRole;
using Users.Application.Roles.Commands.DeleteRole;
using Users.Application.Roles.Commands.RemovePermissionFromRole;
using Users.Application.Roles.Queries.GetRoleById;
using Users.Application.Roles.Queries.ListRoles;

namespace Users.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin")]
	public partial class RolesController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ILogger<RolesController> _logger;

		public RolesController(IMediator mediator, ILogger<RolesController> logger)
		{
			_mediator = mediator;
			_logger = logger;
		}

		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<RoleDto>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetAll(CancellationToken ct)
		{
			var result = await _mediator.Send(new ListRolesQuery(), ct);
			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				_logger.LogWarning("ListRolesQuery failed: {Errors}", errors);
				return BadRequest(new { errors });
			}

			return Ok(result.Value);
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(typeof(RoleDto), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
		{
			var result = await _mediator.Send(new GetRoleByIdQuery(id), ct);
			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(m => m.Contains("not found", StringComparison.OrdinalIgnoreCase)))
					return NotFound();
				return BadRequest(new { errors });
			}

			return Ok(result.Value);
		}

		[HttpPost]
		[ProducesResponseType(typeof(RoleDto), (int)HttpStatusCode.Created)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var cmd = new CreateRoleCommand(request.Name, request.PermissionIds);
			var result = await _mediator.Send(cmd, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				return BadRequest(new { errors });
			}

			var dto = result.Value;
			return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
		}

		[HttpDelete("{id:guid}")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
		{
			var result = await _mediator.Send(new DeleteRoleCommand(id), ct);
			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(m => m.Contains("not found", StringComparison.OrdinalIgnoreCase)))
					return NotFound();
				return BadRequest(new { errors });
			}

			return NoContent();
		}

		/// <summary>
		/// Add single permission to role.
		/// </summary>
		[HttpPost("{id:guid}/permissions/{permissionId:guid}")]
		[ProducesResponseType(typeof(RoleDto), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> AddPermission(Guid id, Guid permissionId, CancellationToken ct)
		{
			var cmd = new AddPermissionToRoleCommand(id, permissionId);
			var result = await _mediator.Send(cmd, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(m => m.Contains("not found", StringComparison.OrdinalIgnoreCase)))
					return NotFound();
				return BadRequest(new { errors });
			}

			return Ok(result.Value);
		}

		/// <summary>
		/// Remove single permission from role.
		/// </summary>
		[HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
		[ProducesResponseType(typeof(RoleDto), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> RemovePermission(Guid id, Guid permissionId, CancellationToken ct)
		{
			var cmd = new RemovePermissionFromRoleCommand(id, permissionId);
			var result = await _mediator.Send(cmd, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(m => m.Contains("not found", StringComparison.OrdinalIgnoreCase)))
					return NotFound();
				return BadRequest(new { errors });
			}

			return Ok(result.Value);
		}
	}
}
