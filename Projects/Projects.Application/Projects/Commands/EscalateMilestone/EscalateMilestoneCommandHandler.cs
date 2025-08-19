using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.EscalateMilestone
{
	public class EscalateMilestoneCommandHandler : IRequestHandler<EscalateMilestoneCommand, Result>
	{
		private readonly IProjectRepository _projectRepository;
		private readonly ILogger<EscalateMilestoneCommandHandler> _logger;
		private readonly IProjectQueryService _projectQueryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public EscalateMilestoneCommandHandler(IProjectRepository projectRepository,
			ILogger<EscalateMilestoneCommandHandler> logger,
			IProjectQueryService projectQueryService,
			IUnitOfWork unitOfWork,
			IMapper mapper)
		{
			_projectRepository = projectRepository;
			_logger = logger;
			_projectQueryService = projectQueryService;
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Result> Handle(EscalateMilestoneCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Handling EscalateMilestoneCommand for ProjectId: {ProjectId}", request.ProjectId);
			try
			{
				var project = await _projectQueryService.GetByIdWithMilestonesAsync(request.ProjectId, cancellationToken);
				if (project is null)
				{
					_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
					return Result.Fail("Project not found.");
				}

				project.CheckEscalatedMilestones();

				await _projectRepository.UpdateAsync(project, cancellationToken);
				_unitOfWork.TrackEntity(project);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				_logger.LogInformation("Milestones in project {ProjectId} escalated", project.Id);

				return Result.Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error escalating milestones in project {ProjectId}", request.ProjectId);
				return Result.Fail("An error occurred while escalating the milestones.");
			}
		}
	}
}
