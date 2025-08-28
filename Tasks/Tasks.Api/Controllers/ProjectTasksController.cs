using FluentResults;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Api.Models.Requests;
using Tasks.Application.Common;
using Tasks.Application.Common.Filters;
using Tasks.Application.Common.Pagination;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Commands;
using Tasks.Application.ProjectTasks.Commands.AddTimeEntry;
using Tasks.Application.ProjectTasks.Commands.AssignProjectTask;
using Tasks.Application.ProjectTasks.Commands.CancelProjectTask;
using Tasks.Application.ProjectTasks.Commands.CompleteProjectTask;
using Tasks.Application.ProjectTasks.Commands.CreateProjectTask;
using Tasks.Application.ProjectTasks.Commands.DeleteProjectTask;
using Tasks.Application.ProjectTasks.Commands.StartProjectTask;
using Tasks.Application.ProjectTasks.Commands.UpdateProjectTask;
using Tasks.Application.ProjectTasks.Queries;
using Tasks.Application.ProjectTasks.Queries.GetProjectTaskById;
using Tasks.Application.ProjectTasks.Queries.GetTasks;

namespace Tasks.Api.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class ProjectTasksController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ICurrentUserService _currentUserService;
		private readonly ILogger<ProjectTasksController> _logger;

		public ProjectTasksController(
			IMediator mediator,
			ICurrentUserService currentUserService,
			ILogger<ProjectTasksController> logger)
		{
			_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
			_currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpPost]
		[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CreateTask([FromBody] CreateProjectTaskRequest request, CancellationToken ct)
		{
			_logger.LogInformation("Received request to create a task by user {UserId}", _currentUserService.UserId);

			var command = request.Adapt<CreateProjectTaskCommand>() with
			{
				ReporterId = _currentUserService.UserId
			};

			var result = await _mediator.Send(command, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				_logger.LogWarning("Failed to create task. Errors: {Errors}", errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Task {TaskId} successfully created by user {UserId}", result.Value, _currentUserService.UserId);
			return CreatedAtAction(nameof(GetTaskById), new { taskId = result.Value }, result.Value);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginatedResult<TaskListItemDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetTasks([FromQuery] TaskFilter filter, [FromQuery] PaginationInfo paginationInfo, CancellationToken ct)
		{
			_logger.LogInformation("Fetching tasks with filter {@Filter} and pagination {@Pagination}", filter, paginationInfo);

			try
			{
				var result = await _mediator.Send(new GetProjectTasksQuery(filter, paginationInfo));

				if (result.IsFailed)
				{
					return BadRequest(result.Errors);
				}

				_logger.LogInformation("Tasks fetched. Total: {Total}", result.Value.Pagination.TotalItems);

				return Ok(result.Value);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to fetch tasks");
				return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
			}
		}

		[HttpGet("{taskId:guid}")]
		[ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetTaskById(Guid taskId, [FromQuery] string[] includes, CancellationToken ct)
		{

			_logger.LogInformation("Fetching task with ID: {TaskId}", taskId);

			var includeOptions = includes
				.Select(i => Enum.TryParse<TaskIncludeOptions>(i, true, out var option) ? option : (TaskIncludeOptions?)null)
				.Where(o => o.HasValue)
				.Select(o => o.Value)
				.ToList();

			if (taskId == Guid.Empty)
			{
				_logger.LogWarning("GetTaskById called with empty TaskId");
				return BadRequest(new { errors = new[] { "TaskId is required." } });
			}

			var result = await _mediator.Send(new GetProjectTaskByIdQuery(taskId, includeOptions), ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				_logger.LogWarning("Task {TaskId} not found. Errors: {Errors}", taskId, errors);
				return NotFound(new { errors });
			}

			_logger.LogInformation("Task {TaskId} retrieved successfully", taskId);
			return Ok(result.Value);
		}

		[HttpPut("{taskId:guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateProjectTaskRequest request, CancellationToken ct)
		{
			_logger.LogInformation("Received request to update task with ID: {TaskId}", taskId);

			if (taskId == Guid.Empty)
			{
				_logger.LogWarning("UpdateTask called with empty TaskId");
				return BadRequest(new { errors = new[] { "TaskId is required." } });
			}

			var command = request.Adapt<UpdateProjectTaskCommand>() with { TaskId = taskId };

			var result = await _mediator.Send(command, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();

				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Task {TaskId} not found. Errors: {Errors}", taskId, errors);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to update task {TaskId}. Errors: {Errors}", taskId, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Task {TaskId} successfully updated", taskId);
			return NoContent();
		}

		[HttpDelete("{taskId:guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken ct)
		{
			_logger.LogInformation("Received request to delete task with ID: {TaskId}", taskId);

			if (taskId == Guid.Empty)
			{
				_logger.LogWarning("DeleteTask called with empty TaskId");
				return BadRequest(new { errors = new[] { "TaskId is required." } });
			}

			var result = await _mediator.Send(new DeleteProjectTaskCommand(taskId), ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Task {TaskId} not found", taskId);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to delete task {TaskId}. Errors: {Errors}", taskId, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Task {TaskId} successfully deleted", taskId);
			return NoContent();
		}

		[HttpPatch("{taskId:guid}/assign")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> AssignTask(Guid taskId, [FromBody] AssignTaskRequest request, CancellationToken ct)
		{
			_logger.LogInformation("Received request to assign task {TaskId} to user {AssigneeId}", taskId, request.AssigneeId);

			if (taskId == Guid.Empty)
				return BadRequest(new { errors = new[] { "TaskId is required." } });

			if (request.AssigneeId == Guid.Empty)
				return BadRequest(new { errors = new[] { "AssigneeId is required." } });

			var command = new AssignProjectTaskCommand(taskId, request.AssigneeId);
			var result = await _mediator.Send(command, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Task {TaskId} not found for assign", taskId);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to assign task {TaskId}. Errors: {Errors}", taskId, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Task {TaskId} assigned to {AssigneeId} by user {UserId}", taskId, request.AssigneeId, _currentUserService.UserId);
			return NoContent();
		}

		[HttpPatch("{taskId:guid}/start")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> StartTask(Guid taskId, CancellationToken ct)
		{
			_logger.LogInformation("Received request to start task {TaskId} by user {UserId}", taskId, _currentUserService.UserId);

			if (taskId == Guid.Empty)
				return BadRequest(new { errors = new[] { "TaskId is required." } });

			var command = new StartProjectTaskCommand(taskId);
			var result = await _mediator.Send(command, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Task {TaskId} not found for start", taskId);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to start task {TaskId}. Errors: {Errors}", taskId, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Task {TaskId} moved to InProgress by user {UserId}", taskId, _currentUserService.UserId);
			return NoContent();
		}

		[HttpPatch("{taskId:guid}/complete")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> CompleteTask(Guid taskId, CancellationToken ct)
		{
			_logger.LogInformation("Received request to complete task {TaskId} by user {UserId}", taskId, _currentUserService.UserId);

			if (taskId == Guid.Empty)
				return BadRequest(new { errors = new[] { "TaskId is required." } });

			var command = new StartProjectTaskCommand(taskId);
			var result = await _mediator.Send(command, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Task {TaskId} not found for complete", taskId);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to complete task {TaskId}. Errors: {Errors}", taskId, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Task {TaskId} marked as Completed by user {UserId}", taskId, _currentUserService.UserId);
			return NoContent();
		}

		[HttpPatch("{taskId:guid}/cancel")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> CancelTask(Guid taskId, [FromBody] CancelTaskRequest request, CancellationToken ct)
		{
			_logger.LogInformation("Received request to cancel task {TaskId} by user {UserId}", taskId, _currentUserService.UserId);

			if (taskId == Guid.Empty)
				return BadRequest(new { errors = new[] { "TaskId is required." } });

			if (string.IsNullOrWhiteSpace(request.Reason))
				return BadRequest(new { errors = new[] { "Reason is required." } });

			var command = new CancelProjectTaskCommand(taskId, request.Reason);
			var result = await _mediator.Send(command, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Task {TaskId} not found for cancel", taskId);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to cancel task {TaskId}. Errors: {Errors}", taskId, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Task {TaskId} successfully cancelled by user {UserId}", taskId, _currentUserService.UserId);
			return NoContent();
		}

		[HttpPost("{taskId:guid}/time-entries")]
		[ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> AddTimeEntry(Guid taskId, [FromBody] AddTimeEntryRequest request, CancellationToken ct)
		{
			_logger.LogInformation("Received request to add time entry to task {TaskId} by user {UserId}", taskId, _currentUserService.UserId);

			if (taskId == Guid.Empty)
				return BadRequest(new { errors = new[] { "TaskId is required." } });

			var command = request.Adapt<LogTimeCommand>() with
			{
				TaskId = taskId,
				UserId = _currentUserService.UserId
			};

			var result = await _mediator.Send(command, ct);

			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToList();
				if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
				{
					_logger.LogWarning("Task {TaskId} not found for adding time entry", taskId);
					return NotFound(new { errors });
				}

				_logger.LogWarning("Failed to add time entry to task {TaskId}. Errors: {Errors}", taskId, errors);
				return BadRequest(new { errors });
			}

			_logger.LogInformation("Time entry {TimeEntryId} added to task {TaskId} by user {UserId}", result.Value, taskId, _currentUserService.UserId);
			return Ok(result.Value);
		}
	}
}
