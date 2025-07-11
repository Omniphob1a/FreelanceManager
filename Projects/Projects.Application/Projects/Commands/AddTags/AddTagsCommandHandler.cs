using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.AddTag;
using Projects.Domain.Exceptions;
using Projects.Application.Services;
using Projects.Domain.Repositories;

namespace Projects.Application.Projects.Commands.AddTag;

public class AddTagsCommandHandler : IRequestHandler<AddTagsCommand, Result>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IProjectQueryService _queryService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<AddTagsCommandHandler> _logger;
	private readonly TagParserService _tagParserService;

	public AddTagsCommandHandler(
		IProjectRepository projectRepository,
		IProjectQueryService queryService,
		IUnitOfWork unitOfWork,
		ILogger<AddTagsCommandHandler> logger,
		TagParserService tagParserService)
	{
		_projectRepository = projectRepository;
		_queryService = queryService;
		_unitOfWork = unitOfWork;
		_logger = logger;
		_tagParserService = tagParserService;
	}

	public async Task<Result> Handle(AddTagsCommand request, CancellationToken ct)
	{
		_logger.LogDebug("Handling AddTagsCommand for ProjectId: {ProjectId}", request.ProjectId);

		var project = await _queryService.GetByIdAsync(request.ProjectId, ct);
		if (project is null)
		{
			_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
			return Result.Fail("Project not found.");
		}

		var tagsResult = _tagParserService.ParseTags(request.Tags);
		if (tagsResult.IsFailed)
		{
			_logger.LogWarning("Tag parsing failed for ProjectId {ProjectId}: {Errors}", request.ProjectId, tagsResult.Errors);
			return Result.Fail(tagsResult.Errors);
		}

		try
		{
			foreach (var tag in tagsResult.Value)
			{
				project.AddTag(tag);
			}

			await _projectRepository.UpdateAsync(project, ct);
			await _unitOfWork.SaveChangesAsync(ct);
			_logger.LogInformation("Tags [{Tags}] added to Project {ProjectId}", string.Join(", ", tagsResult.Value), project.Id);

			return Result.Ok();
		}
		catch (DomainException ex)
		{
			_logger.LogError(ex, "Domain error while adding tags to Project {ProjectId}", project.Id);
			return Result.Fail(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unexpected error while adding tags to Project {ProjectId}", project.Id);
			return Result.Fail("Unexpected error.");
		}
	}
}
