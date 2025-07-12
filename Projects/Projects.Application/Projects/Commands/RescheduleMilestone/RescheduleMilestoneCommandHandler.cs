using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.RescheduleMilestone;
using Projects.Domain.Entities.ProjectService.Domain.Entities;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.RescheduleMilestone
{
	public class RescheduleMilestoneCommandHandler : IRequestHandler<RescheduleMilestoneCommand, Result>
	{
		private readonly IProjectRepository _repository;
		private readonly IProjectQueryService _queryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<RescheduleMilestoneCommandHandler> _logger;
		private readonly IMapper _mapper;

		public RescheduleMilestoneCommandHandler(IProjectRepository repository,
			IProjectQueryService queryService,
			IUnitOfWork unitOfWork,
			ILogger<RescheduleMilestoneCommandHandler> logger,
			IMapper mapper)
		{
			_repository = repository;
			_queryService = queryService;
			_unitOfWork = unitOfWork;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<Result> Handle(RescheduleMilestoneCommand request, CancellationToken ct)
		{
			_logger.LogDebug("Handling RescheduleMilestoneCommand for ProjectId: {ProjectId}", request.ProjectId);

			try
			{
				var project = await _queryService.GetByIdWithMilestonesAsync(request.ProjectId, ct);
				if (project is null)
				{
					_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
					return Result.Fail("Project not found.");
				}

				var milestone = project.Milestones.FirstOrDefault(m => m.Id == request.MilestoneId);
				if (milestone is null)
				{
					_logger.LogWarning("Milestone {MilestoneId} not found", request.MilestoneId);
					return Result.Fail("Milestone not found.");
				}
				milestone.Reschedule(request.NewDueDate);

				await _repository.UpdateAsync(project, ct);
				await _unitOfWork.SaveChangesAsync(ct);

				_logger.LogInformation("Milestone {MilestoneId} Rescheduled in project {ProjectId}", milestone.Id, project.Id);

				return Result.Ok();
			}
			catch (DomainException ex)
			{
				_logger.LogError(ex, "Domain error while rescheduling milestone {MiletoneId} in project {ProjectId}", request.MilestoneId, request.ProjectId);
				return Result.Fail(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, "Unexpected error while rescheduling  milestone {MiletoneId} in project {ProjectId}", request.MilestoneId, request.ProjectId);
				return Result.Fail("Unexpected error occurred.");
			}
		}
	}

}
