using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.CompleteMilestone;
using Projects.Domain.Entities;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.CompleteMilestone
{
	public class CompleteMilestoneCommandHandler : IRequestHandler<CompleteMilestoneCommand, Result>
	{
		private readonly IProjectRepository _repository;
		private readonly IProjectQueryService _queryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<CompleteMilestoneCommandHandler> _logger;
		private readonly IMapper _mapper;

		public CompleteMilestoneCommandHandler(IProjectRepository repository,
			IProjectQueryService queryService,
			IUnitOfWork unitOfWork,
			ILogger<CompleteMilestoneCommandHandler> logger,
			IMapper mapper)
		{
			_repository = repository;
			_queryService = queryService;
			_unitOfWork = unitOfWork;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<Result> Handle(CompleteMilestoneCommand request, CancellationToken ct)
		{
			_logger.LogDebug("Handling CompleteMilestoneCommand for ProjectId: {ProjectId}", request.ProjectId);

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
				project.CompleteMilestone(milestone.Id);

				await _repository.UpdateAsync(project, ct);
				_unitOfWork.TrackEntity(project);
				await _unitOfWork.SaveChangesAsync(ct);

				_logger.LogInformation("Milestone {MilestoneId} completed in project {ProjectId}", milestone.Id, project.Id);

				return Result.Ok();
			}
			catch (DomainException ex)
			{
				_logger.LogError(ex, "Domain error while completing milestone {MiletoneId} in project {ProjectId}",request.MilestoneId, request.ProjectId);
				return Result.Fail(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, "Unexpected error while completing milestone {MiletoneId} in project {ProjectId}", request.MilestoneId, request.ProjectId);
				return Result.Fail("Unexpected error occurred.");
			}
		}
	}

}
