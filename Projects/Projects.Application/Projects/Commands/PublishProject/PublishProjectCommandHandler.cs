using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Notifications;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.CompleteProject;
using Projects.Domain.Enums;
using Projects.Domain.Events;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.PublishProject;

public class PublishProjectCommandHandler : IRequestHandler<PublishProjectCommand, Result>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<CompleteProjectCommandHandler> _logger;


	public PublishProjectCommandHandler(
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

	public async Task<Result> Handle(PublishProjectCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling PublishProjectCommand for ProjectId: {ProjectId}", request.ProjectId);

		try
		{
			var project = await _queryService.GetByIdAsync(request.ProjectId, ct);
			if (project is null)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail("Project not found.");
			}

			if (project.Status == ProjectStatus.Active)
			{
				_logger.LogInformation("Project {ProjectId} is already published", request.ProjectId);
				return Result.Ok(); 
			}

			project.Publish(request.ExpiresAt);
			await _projectRepository.UpdateAsync(project, ct);
			_unitOfWork.TrackEntity(project);
			await _unitOfWork.SaveChangesAsync(ct);
			_logger.LogInformation("Project {ProjectId} published successfully", request.ProjectId);

			return Result.Ok();
		}
		catch (DomainException ex)
		{
			_logger.LogWarning(ex, "Domain error while publishing project {ProjectId}", request.ProjectId);
			return Result.Fail(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while publishing project {ProjectId}", request.ProjectId);
			return Result.Fail("Unexpected error.");
		}
	}
}
