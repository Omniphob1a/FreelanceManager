using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.RescheduleAllMilestones
{
	public class RescheduleAllMilestonesCommandHandler : IRequestHandler<RescheduleAllMilestonesCommand, Result>
	{
		private readonly IProjectRepository _repository;
		private readonly IProjectQueryService _queryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<RescheduleAllMilestonesCommandHandler> _logger;

		public RescheduleAllMilestonesCommandHandler(IProjectRepository repository, 
			IProjectQueryService projectQueryService, 
			IUnitOfWork unitOfWork,
			ILogger<RescheduleAllMilestonesCommandHandler> logger)
		{
			_repository = repository;
			_queryService = projectQueryService;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<Result> Handle(RescheduleAllMilestonesCommand request, CancellationToken ct)
		{
			_logger.LogDebug("Handling RescheduleAllMilestonesCommand for ProjectId: {ProjectId}", request.ProjectId);

			try
			{
				var project = await _queryService.GetByIdWithMilestonesAsync(request.ProjectId, ct);
				if (project is null)
				{
					_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
					return Result.Fail("Project not found.");
				}

				var newDueDate = DateTime.UtcNow.Add(request.Extension);

				foreach (var milestone in project.Milestones)
				{
					if (milestone.IsCompleted)
					{
						_logger.LogWarning("Milestone {MilestoneId} is already completed, skipping reschedule", milestone.Id);
						continue;
					}
					milestone.Reschedule(newDueDate);
				}

				await _repository.UpdateAsync(project, ct);
				_unitOfWork.TrackEntity(project);
				await _unitOfWork.SaveChangesAsync(ct);

				_logger.LogInformation("Milestones rescheduled in project {ProjectId}", project.Id);

				return Result.Ok();
			}
			catch (DomainException ex)
			{
				_logger.LogError(ex, "Domain error while rescheduling milestones in project {ProjectId}",request.ProjectId);
				return Result.Fail(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, "Unexpected error while rescheduling  milestones in project {ProjectId}", request.ProjectId);
				return Result.Fail("Unexpected error occurred.");
			}
		}
	}
}
