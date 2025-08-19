using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Enums;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.CompleteProject;

public class CompleteProjectCommandHandler : IRequestHandler<CompleteProjectCommand, Result>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<CompleteProjectCommandHandler> _logger;

	public CompleteProjectCommandHandler(
		IProjectRepository projectRepository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<CompleteProjectCommandHandler> logger)
	{
		_projectRepository = projectRepository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<Result> Handle(CompleteProjectCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling CompleteProjectCommand for ProjectId: {ProjectId}", request.ProjectId);

		try
		{
			var project = await _queryService.GetByIdAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail("Project not found.");
			}

			if (project.Status == ProjectStatus.Completed)
			{
				_logger.LogInformation("Project {ProjectId} is already completed", request.ProjectId);
				return Result.Ok(); 
			}

			_logger.LogInformation("Project status before domain method call: {Status}", project.Status);
			project.Complete();

			await _projectRepository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);
			await _unitOfWork.SaveChangesAsync(ct);
			_logger.LogInformation("Project {ProjectId} marked as completed", request.ProjectId);

			return Result.Ok();
		}
		catch (DomainException ex)
		{
			_logger.LogWarning(ex, "Domain error while completing project {ProjectId}", request.ProjectId);
			return Result.Fail(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while completing project {ProjectId}", request.ProjectId);
			return Result.Fail("Unexpected error occurred.");
		}
	}
}
