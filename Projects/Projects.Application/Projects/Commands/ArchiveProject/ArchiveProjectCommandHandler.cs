using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Enums;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.ArchiveProject;

public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, Result>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<ArchiveProjectCommandHandler> _logger;

	public ArchiveProjectCommandHandler(
		IProjectRepository projectRepository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<ArchiveProjectCommandHandler> logger)
	{
		_projectRepository = projectRepository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<Result> Handle(ArchiveProjectCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling ArchiveProjectCommand for ProjectId: {ProjectId}", request.ProjectId);

		try
		{
			var project = await _queryService.GetByIdAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail("Project not found.");
			}

			if (project.Status == ProjectStatus.Archived)
			{
				_logger.LogInformation("Project {ProjectId} is already archived", request.ProjectId);
				return Result.Ok(); 
			}

			project.Archive();

			await _projectRepository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);
			await _unitOfWork.SaveChangesAsync(ct);
			_logger.LogInformation("Project {ProjectId} archived successfully", request.ProjectId);

			return Result.Ok();
		}
		catch (DomainException ex)
		{
			_logger.LogWarning(ex, "Domain error while archiving Project {ProjectId}", request.ProjectId);
			return Result.Fail(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while archiving Project {ProjectId}", request.ProjectId);
			return Result.Fail("Unexpected error occurred.");
		}
	}
}
