using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.AddTimeEntry
{
	public class AddTimeEntryCommandHandler : IRequestHandler<AddTimeEntryCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<AddTimeEntryCommandHandler> _logger;

		public AddTimeEntryCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			IUnitOfWork unitOfWork,
			ILogger<AddTimeEntryCommandHandler> logger)
		{
			_projectTaskRepository = projectTaskRepository;
			_projectTaskQueryService = projectTaskQueryService;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<Result<Unit>> Handle(AddTimeEntryCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Adding time entry for TaskId: {TaskId}, UserId: {UserId}, Start: {Start}, End: {End}",
				request.TaskId, request.UserId, request.Start, request.End);

			var task = await _projectTaskQueryService.GetByIdAsync(request.TaskId, cancellationToken);
			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found when adding time entry", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			try
			{
				var period = TimeRange.Create(request.Start, request.End);
				Money? rate = null;

				if (request.HourlyRate.HasValue && !string.IsNullOrWhiteSpace(request.Currency))
				{
					rate = Money.From(request.HourlyRate.Value, request.Currency!);
				}

				var entry = TimeEntry.Create(
					request.UserId,
					request.TaskId,
					period,
					request.Description,
					request.IsBillable,
					rate,
					DateTime.UtcNow
				);

				task.AddTimeEntry(entry);

				await _projectTaskRepository.UpdateAsync(task, cancellationToken);
				_unitOfWork.TrackEntity(task);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Validation error while adding time entry to Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(ex.Message);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while adding time entry to Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while adding time entry to Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>("Unexpected error occurred while adding time entry.");
			}

			_logger.LogInformation("Time entry successfully added to Task {TaskId}", request.TaskId);
			return Result.Ok();
		}
	}
}
