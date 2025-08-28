using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Users.Application.Contracts;
using Users.Application.DTOs;
using Users.Application.Responses;
using Users.Application.Users.Commands.ChangeUserLogin;
using Users.Application.Users.Commands.ChangeUserPassword;
using Users.Application.Users.Commands.DeleteUser;
using Users.Application.Users.Commands.RegisterUser;
using Users.Application.Users.Commands.RestoreUser;
using Users.Application.Users.Commands.UpdateUser;
using Users.Application.Users.Queries.AuthenticateUser;
using Users.Application.Users.Queries.GetAllActiveUsers;
using Users.Application.Users.Queries.GetUserByCredentials;
using Users.Application.Users.Queries.GetUserByEmail;
using Users.Application.Users.Queries.GetUserByLogin;
using Users.Application.Users.Queries.GetUsersByAge;
using Users.Domain.ValueObjects;
using Users.Infrastructure.Auth;

namespace Users.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly IMediator _mediator;
		public UsersController(IMediator mediator) => _mediator = mediator;

		[HttpPost]
		[Authorize(Roles = "Admin")]
		[AuthorizePermission("ManageUsers")]
		public async Task<IActionResult> Create([FromBody] RegisterUserCommand cmdDto)
		{
			var actor = User.Identity?.Name
						?? throw new InvalidOperationException("Name cannot be empty");
			var cmd = cmdDto with { CreatedBy = actor };
			var result = await _mediator.Send(cmd);
			return result.IsSuccess
				 ? CreatedAtAction(nameof(GetByLogin), new { login = cmd.Login }, null)
				 : BadRequest(result.Errors);
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAll()
		{
			var users = await _mediator.Send(new GetAllActiveUsersQuery());
			return Ok(users);
		}

		[HttpGet("by-login/{login}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetByLogin(string login)
		{
			try
			{
				UserDto dto = await _mediator.Send(new GetUserByLoginQuery(login));
				return Ok(dto);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}
		[HttpGet("by-email/{email}")]
		[Authorize(Roles = "User")]
		public async Task<IActionResult> GetByEmail(string email)
		{
			try
			{
				UserDto dto = await _mediator.Send(new GetUserByEmailQuery(email));
				return Ok(dto);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}

		[HttpGet("age/{minAge}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetByAge(int minAge)
		{
			var users = await _mediator.Send(new GetUsersByAgeQuery(minAge));
			return Ok(users);
		}

		[HttpPut("{userId:guid}")]
		public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateUserCommand cmdDto)
		{
			var actor = User.Identity?.Name!;
			var cmd = new UpdateUserRequest(
				UserId: userId,
				Command: cmdDto with { ModifiedBy = actor } 
			);
			var result = await _mediator.Send(cmd);
			return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
		}

		[HttpPut("{userId:guid}/password")]
		public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangeUserPasswordCommand cmdDto)
		{
			var actor = User.Identity?.Name!;
			var cmd = cmdDto with { ModifiedBy = actor };
			var result = await _mediator.Send(new ChangeUserPasswordRequest(userId, cmd));
			return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
		}

		[HttpPut("{userId:guid}/login")]
		public async Task<IActionResult> ChangeLogin(Guid userId, [FromBody] ChangeUserLoginCommand cmdDto)
		{
			var actor = User.Identity?.Name!;
			var cmd = cmdDto with { ModifiedBy = actor };
			var result = await _mediator.Send(new ChangeUserLoginRequest(userId, cmd));
			return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
		}

		[HttpDelete("{userId:guid}")]
		[Authorize(Roles = "Admin")]
		[AuthorizePermission("DeleteUser")]
		public async Task<IActionResult> Delete(Guid userId, [FromQuery] bool hard = false)
		{
			var actor = User.Identity?.Name!;
			var cmd = new DeleteUserCommand(userId, hard, RevokedBy: actor);
			var result = await _mediator.Send(cmd);
			return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
		}

		[HttpPatch("{userId:guid}/restore")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Restore(Guid userId)
		{
			var actor = User.Identity?.Name!;
			var cmd = new RestoreUserCommand(userId, ModifiedBy: actor);
			var result = await _mediator.Send(cmd);
			return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
		}

		[HttpPost("get-my-profile")]
		[Authorize]
		public async Task<IActionResult> GetProfile()
		{
			var login = User.Identity?.Name;
			if (string.IsNullOrEmpty(login)) return Unauthorized();

			var result = await _mediator.Send(new GetUserByLoginQuery(login));
			return Ok(result);
		}

	}
}
