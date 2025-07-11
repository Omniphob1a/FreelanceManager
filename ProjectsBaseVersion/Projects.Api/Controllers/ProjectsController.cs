using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Projects.Api.Models.Requests;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands;
using Projects.Application.Projects.Commands.CreateProject;
using Projects.Application.Projects.Commands.DeleteProject;
using Projects.Application.Projects.Queries.GetActiveProjects;
using Projects.Application.Projects.Queries.GetProjectById;
using Projects.Domain.Entities;
using Projects.Shared.Extensions;

namespace Projects.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProjectsController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ICurrentUserService _currentUserService;
		private readonly ILogger<ProjectsController> _logger;

		public ProjectsController(IMediator mediator, ICurrentUserService currentUserService, ILogger<ProjectsController> logger)
		{
			_mediator = mediator;
			_currentUserService = currentUserService;
			_logger = logger;
		}

		[HttpPost]
		[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
		{
			_logger.LogInformation("Received request to create a project by user {UserId}", _currentUserService.UserId);

			var command = request.Adapt<CreateProjectCommand>() with
			{
				OwnerId = _currentUserService.UserId
			};

			var result = await _mediator.Send(command);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message);
				_logger.LogWarning("Failed to create project. Errors: {Errors}", errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Project {ProjectId} successfully created by user {UserId}", result.Value, _currentUserService.UserId);
			return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Value }, result.Value);
		}

		[HttpGet("{projectId:guid}")]
		[ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetProjectById(Guid projectId)
		{
			_logger.LogInformation("Fetching project with ID: {ProjectId}", projectId);

			var result = await _mediator.Send(new GetProjectByIdQuery(projectId));

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message);
				_logger.LogWarning("Project {ProjectId} not found. Errors: {Errors}", projectId, errors);
				return NotFound(new { errors });
			}

			_logger.LogInformation("Project {ProjectId} retrieved successfully", projectId);
			return Ok(result.Value);
		}

		[HttpGet("get-active")]
		[ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetActiveProjects()
		{
			_logger.LogInformation("Fetching active projects");

			var result = await _mediator.Send(new GetActiveProjectsQuery());

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message);
				_logger.LogWarning("Failed to retrieve active projects. Errors: {Errors}", errors);
				return NotFound(new { errors });
			}

			_logger.LogInformation("Successfully retrieved active projects");
			return Ok(result.Value);
		}

		[HttpDelete("{projectId:guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteProject(Guid projectId)
		{
			_logger.LogInformation("Received request to delete project with ID: {ProjectId}", projectId);

			var command = new DeleteProjectCommand(projectId);
			var result = await _mediator.Send(command);

			if (result.IsFailed)
			{
				var errorMessages = result.Errors.Select(e => e.Message).ToList();

				if (errorMessages.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Project with ID {ProjectId} not found", projectId);
					return NotFound(new { errors = errorMessages });
				}

				_logger.LogWarning("Failed to delete project {ProjectId}: {Errors}", projectId, errorMessages);
				return BadRequest(new { errors = errorMessages });
			}

			_logger.LogInformation("Project {ProjectId} successfully deleted", projectId);
			return NoContent();
		}

		[HttpPut("{id:guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
		{
			_logger.LogInformation("Received request to update project with ID: {ProjectId}", id);

			var command = new UpdateProjectCommand
			{
				Id = id,
				Title = request.Title,
				Description = request.Description,
				OwnerId = request.OwnerId,
				BudgetMin = request.BudgetMin,
				BudgetMax = request.BudgetMax,
				Currency = request.Currency,
				Category = request.Category,
				Tags = request.Tags
			};

			var result = await _mediator.Send(command);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();

				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Project {ProjectId} not found. Errors: {Errors}", id, errors);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to update project {ProjectId}. Errors: {Errors}", id, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Project {ProjectId} successfully updated", id);
			return NoContent();
		}
	}
}
