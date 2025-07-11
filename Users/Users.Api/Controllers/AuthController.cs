using Microsoft.AspNetCore.Mvc;
using Users.Application.Interfaces;
using MediatR;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;
using Users.Application.Users.Commands.RegisterUser;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Users.Application.Users.Queries.AuthenticateUser;
using Users.Application.Responses;
using Users.Domain.Interfaces.Repositories;

namespace Users.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IMediator _mediator;
		public AuthController(IMediator mediator, IUserRepository userRepository)
		{
			_mediator = mediator;
		}


		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterUserCommand cmd)
		{
			try
			{
				var result = await _mediator.Send(cmd);
				if (result.IsSuccess)
					return Ok(result.Value);

				return BadRequest(result.Errors);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "Server error");
			}
		}

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] AuthenticateUserQuery query)
		{
			Result<AuthenticationResult> result = await _mediator.Send(query);

			if (result.IsFailed)
			{
				return Unauthorized(result.Errors.First().Message); 
			}

			var token = result.Value.Token;

			Response.Cookies.Append("secretCookie", token);

			if (result.IsSuccess)
				return Ok(result.Value);


			return Unauthorized(result.Errors);
		}
	}
}
