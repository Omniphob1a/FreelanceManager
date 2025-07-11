using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Domain.Entities;
using Projects.Domain.Entities.ProjectService.Domain.Entities;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.AddMilestone;

public class AddMilestoneCommandHandler : IRequestHandler<AddMilestoneCommand, Result<ProjectMilestoneDto>>
{
	private readonly IProjectRepository _repository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<AddMilestoneCommandHandler> _logger;
	private readonly IMapper _mapper;

	public AddMilestoneCommandHandler(IProjectRepository repository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<AddMilestoneCommandHandler> logger,
		IMapper mapper)
	{
		_repository = repository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
		_mapper = mapper;
	}

	public async Task<Result<ProjectMilestoneDto>> Handle(AddMilestoneCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling AddMilestoneCommand for ProjectId: {ProjectId}", request.ProjectId);

		try
		{
			var project = await _queryService.GetByIdWithMilestonesAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail<ProjectMilestoneDto>("Project not found.");
			}

			var milestone = new ProjectMilestone(request.Title, request.DueDate, request.ProjectId);
			project.AddMilestone(milestone);

			await _repository.UpdateAsync(project, ct);
			await _unitOfWork.SaveChangesAsync(ct);

			_logger.LogInformation("Milestone {MilestoneId} added to Project {ProjectId}", milestone.Id, project.Id);

			var dto = _mapper.Map<ProjectMilestoneDto>(milestone);
			return Result.Ok(dto);
		}
		catch (DomainException ex)
		{
			_logger.LogError(ex, "Domain error while adding milestone to Project {ProjectId}", request.ProjectId);
			return Result.Fail<ProjectMilestoneDto>(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unexpected error while adding milestone to Project {ProjectId}", request.ProjectId);
			return Result.Fail<ProjectMilestoneDto>("Unexpected error occurred.");
		}
	}
}
