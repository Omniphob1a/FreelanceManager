using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.DeleteAttachment;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.DeleteMilestone;

public class DeleteMilestoneCommandHandler : IRequestHandler<DeleteMilestoneCommand, Result>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<DeleteMilestoneCommandHandler> _logger;
	private readonly IMapper _mapper;

	public DeleteMilestoneCommandHandler(
		IProjectRepository projectRepository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<DeleteMilestoneCommandHandler> logger,
		IMapper mapper)
	{
		_projectRepository = projectRepository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
		_mapper = mapper;
	}

	public async Task<Result> Handle(DeleteMilestoneCommand request, CancellationToken ct)
	{
		_logger.LogInformation("Handling DeleteMilestoneCommand for ProjectId {ProjectId}", request.ProjectId);

		var project = await _queryService.GetByIdWithMilestonesAsync(request.ProjectId, ct);
		if (project is null)
		{
			_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
			return Result.Fail("Project not found.");
		}

		var milestone = project.Milestones.FirstOrDefault(a => a.Id == request.MilestoneId);
		if (milestone is null)
		{
			_logger.LogWarning("Milestone {MilestoneId} not found", request.MilestoneId);
			return Result.Fail("Milestone not found.");
		}

		try
		{
			project.DeleteMilestone(milestone);

			await _projectRepository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);
			await _unitOfWork.SaveChangesAsync(ct);
			_logger.LogInformation("Milestone {MilestoneId} removed from Project {ProjectId}", milestone.Id, project.Id);

			return Result.Ok();
		}
		catch (DomainException ex)
		{
			_logger.LogWarning(ex, "Domain exception while removing Milestone {MilestoneId}", request.MilestoneId);
			return Result.Fail(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while removing Milestone {MilestoneId}", request.MilestoneId);
			return Result.Fail("Unexpected error.");
		}
	}
}
