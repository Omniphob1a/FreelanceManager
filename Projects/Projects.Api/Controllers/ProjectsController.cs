using FluentResults;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Projects.Api.Models.Requests;
using Projects.Application.Common.Filters;
using Projects.Application.Common.Pagination;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands;
using Projects.Application.Projects.Commands.AddAttachment;
using Projects.Application.Projects.Commands.AddMilestone;
using Projects.Application.Projects.Commands.AddTag;
using Projects.Application.Projects.Commands.ArchiveProject;
using Projects.Application.Projects.Commands.CompleteMilestone;
using Projects.Application.Projects.Commands.CompleteProject;
using Projects.Application.Projects.Commands.CreateProject;
using Projects.Application.Projects.Commands.DeleteAttachment;
using Projects.Application.Projects.Commands.DeleteProject;
using Projects.Application.Projects.Commands.DeleteTags;
using Projects.Application.Projects.Commands.PublishProject;
using Projects.Application.Projects.Commands.RescheduleMilestone;
using Projects.Application.Projects.Commands.UpdateProject;
using Projects.Application.Projects.Queries.GetProjectById;
using Projects.Application.Projects.Queries.GetProjectsByFilter;
using Projects.Shared.Extensions;

namespace Projects.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILogger<ProjectsController> _logger;

	public ProjectsController(
		IMediator mediator,
		ICurrentUserService currentUserService,
		ILogger<ProjectsController> logger)
	{
		_mediator = mediator;
		_currentUserService = currentUserService;
		_logger = logger;
	}

	[HttpPost]
	[Authorize]
	[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to create a project by user {UserId}", _currentUserService.UserId);

		var command = request.Adapt<CreateProjectCommand>() with
		{
			OwnerId = _currentUserService.UserId
		};

		var result = await _mediator.Send(command, ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message);
			_logger.LogWarning("Failed to create project. Errors: {Errors}", errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Project {ProjectId} successfully created by user {UserId}", result.Value, _currentUserService.UserId);
		return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Value }, result.Value);
	}

	[HttpGet]
	[ProducesResponseType(typeof(PaginatedResult<ProjectDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetProjects([FromQuery] ProjectFilter filter, CancellationToken ct)
	{
		_logger.LogInformation("Fetching projects with filter {@Filter}", filter);

		var result = await _mediator.Send(new GetProjectsByFilterQuery(filter), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message);
			_logger.LogWarning("Projects not found. Errors: {Errors}", errors);
			return NotFound(new { errors });
		}

		_logger.LogInformation("Projects fetched successfully. Total: {Total}", result.Value.Pagination.TotalItems);
		return Ok(result.Value);
	}

	[HttpGet("{projectId:guid}")]
	[ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetProjectById(Guid projectId, CancellationToken ct)
	{
		_logger.LogInformation("Fetching project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new GetProjectByIdQuery(projectId), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message);
			_logger.LogWarning("Project {ProjectId} not found. Errors: {Errors}", projectId, errors);
			return NotFound(new { errors });
		}

		_logger.LogInformation("Project {ProjectId} retrieved successfully", projectId);
		return Ok(result.Value);
	}

	[HttpDelete("{projectId:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken ct)
	{
		_logger.LogInformation("Received request to delete project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new DeleteProjectCommand(projectId), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project with ID {ProjectId} not found", projectId);
				return NotFound(new { errors });
			}

			_logger.LogWarning("Failed to delete project {ProjectId}: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Project {ProjectId} successfully deleted", projectId);
		return NoContent();
	}

	[HttpPut("{projectId:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateProject(
		Guid projectId,
		[FromBody] UpdateProjectRequest request,
		CancellationToken ct)
	{
		_logger.LogInformation("Received request to update project with ID: {ProjectId}", projectId);

		var command = request.Adapt<UpdateProjectCommand>() with { ProjectId = projectId };

		var result = await _mediator.Send(command, ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project {ProjectId} not found. Errors: {Errors}", projectId, errors);
				return NotFound(new { errors });
			}

			_logger.LogWarning("Failed to update project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Project {ProjectId} successfully updated", projectId);
		return NoContent();
	}

	[HttpPatch("{projectId:guid}/archive")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> ArchiveProject(Guid projectId, CancellationToken ct)
	{
		_logger.LogInformation("Received request to archive project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new ArchiveProjectCommand(projectId), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project {ProjectId} not found. Errors: {Errors}", projectId, errors);
				return NotFound(new { errors });
			}

			_logger.LogWarning("Failed to archive project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Project {ProjectId} successfully archived", projectId);
		return NoContent();
	}

	[HttpPatch("{projectId:guid}/publish")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> PublishProject(Guid projectId, [FromBody] PublishProjectRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to publish project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new PublishProjectCommand(projectId, request.ExpiresAt), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project {ProjectId} not found. Errors: {Errors}", projectId, errors);
				return NotFound(new { errors });
			}

			_logger.LogWarning("Failed to publish project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Project {ProjectId} successfully published", projectId);
		return NoContent();
	}

	[HttpPatch("{projectId:guid}/complete")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> CompleteProject(Guid projectId, CancellationToken ct)
	{
		_logger.LogInformation("Received request to complete project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new CompleteProjectCommand(projectId), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project {ProjectId} not found. Errors: {Errors}", projectId, errors);
				return NotFound(new { errors });
			}

			_logger.LogWarning("Failed to complete project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Project {ProjectId} successfully completed", projectId);
		return NoContent();
	}

	[HttpPatch("{projectId:guid}/add-attachment")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> AddAttachmentToProject(Guid projectId, [FromForm] AddAttachmentRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to add attachment to project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new AddAttachmentCommand(projectId, request.File), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			_logger.LogWarning("Failed to add attachment to project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Attachment to project {ProjectId} successfully added", projectId);
		return Ok(result.Value);
	}

	[HttpPatch("{projectId:guid}/delete-attachment")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]

	public async Task<IActionResult> DeleteAttachmentFromProject(Guid projectId, [FromBody] DeleteAttachmentRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to delete attachment from project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new DeleteAttachmentCommand(projectId, request.AttachmentId), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();
			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project with ID {ProjectId} not found", projectId);
				return NotFound(new { errors });
			}
			_logger.LogWarning("Failed to delete attachment to project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Attachment from project {ProjectId} successfully deleted", projectId);
		return Ok();
	}

	[HttpPatch("{projectId:guid}/add-milestone")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> AddMilestone(Guid projectId, [FromBody] AddMilestoneRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to add milestone to project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new AddMilestoneCommand(projectId, request.Title, request.DueDate), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			_logger.LogWarning("Failed to add milestone project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Milestone to project {ProjectId} successfully added", projectId);
		return Ok(result.Value);
	}

	[HttpPatch("{projectId:guid}/delete-milestone")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> DeleteMilestoneFromProject(Guid projectId, [FromBody] DeleteAttachmentRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to delete milestone from project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new DeleteMilestoneCommand(projectId, request.AttachmentId), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();
			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project with ID {ProjectId} not found", projectId);
				return NotFound(new { errors });
			}
			_logger.LogWarning("Failed to delete milestone to project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Milestone from project {ProjectId} successfully deleted", projectId);
		return Ok();
	}

	[HttpPatch("{projectId:guid}/complete-milestone")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> CompleteMilestoneInProject(Guid projectId, [FromBody] CompleteMilestoneRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to complete milestone {MilestoneId} in project with ID: {ProjectId}", request.MilestoneId, projectId);
		if (request.MilestoneId == Guid.Empty)
		{
			_logger.LogWarning("MilestoneId is empty in the request body");
			return BadRequest(new { errors = new[] { "MilestoneId must be provided." } });
		}
		var result = await _mediator.Send(new CompleteMilestoneCommand(projectId, request.MilestoneId), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Milestone or project not found (ProjectId: {ProjectId}, MilestoneId: {MilestoneId})", projectId, request.MilestoneId);
				return NotFound(new { errors });
			}

			_logger.LogWarning("Failed to complete milestone {MilestoneId} in project {ProjectId}. Errors: {Errors}", request.MilestoneId, projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Milestone {MilestoneId} successfully marked as completed in project {ProjectId}", request.MilestoneId, projectId);
		return Ok();
	}

	[HttpPatch("{projectId:guid}/reschedule-milestone")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RescheduleMilestoneInProject(Guid projectId, [FromBody] RescheduleMilestoneRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to reschedule milestone {MilestoneId} in project {ProjectId} to new due date {NewDueDate}", request.MilestoneId, projectId, request.NewDueDate);

		if (request.MilestoneId == Guid.Empty)
		{
			_logger.LogWarning("MilestoneId is empty in the request body");
			return BadRequest(new { errors = new[] { "MilestoneId must be provided." } });
		}

		if (request.NewDueDate <= DateTime.UtcNow)
		{
			_logger.LogWarning("New due date {NewDueDate} is not in the future", request.NewDueDate);
			return BadRequest(new { errors = new[] { "New due date must be in the future." } });
		}

		var result = await _mediator.Send(new RescheduleMilestoneCommand(projectId, request.MilestoneId, request.NewDueDate), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Milestone or project not found (ProjectId: {ProjectId}, MilestoneId: {MilestoneId})", projectId, request.MilestoneId);
				return NotFound(new { errors });
			}

			_logger.LogWarning("Failed to reschedule milestone {MilestoneId} in project {ProjectId}. Errors: {Errors}", request.MilestoneId, projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Milestone {MilestoneId} successfully rescheduled in project {ProjectId}", request.MilestoneId, projectId);
		return Ok();
	}


	[HttpPatch("{projectId:guid}/add-tags")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> AddTags(Guid projectId, [FromBody] AddTagsRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to add tags to project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new AddTagsCommand(projectId, request.Tags), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();

			_logger.LogWarning("Failed to add tags to project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Tags to project {ProjectId} successfully added", projectId);
		return Ok();
	}

	[HttpPatch("{projectId:guid}/delete-tags")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> DeleteTagsFromProject(Guid projectId, [FromBody] DeleteTagsRequest request, CancellationToken ct)
	{
		_logger.LogInformation("Received request to delete tags from project with ID: {ProjectId}", projectId);

		var result = await _mediator.Send(new DeleteTagsCommand(projectId, request.Tags), ct);

		if (result.IsFailed)
		{
			var errors = result.Errors.Select(e => e.Message).ToList();
			if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogWarning("Project with ID {ProjectId} not found", projectId);
				return NotFound(new { errors });
			}
			_logger.LogWarning("Failed to delete tags from project {ProjectId}. Errors: {Errors}", projectId, errors);
			return BadRequest(new { errors });
		}

		_logger.LogInformation("Tags from project {ProjectId} successfully deleted", projectId);
		return Ok();
	}
}
