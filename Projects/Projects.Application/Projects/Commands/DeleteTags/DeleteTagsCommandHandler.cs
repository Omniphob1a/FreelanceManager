﻿using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Commands.AddTag;
using Projects.Application.Projects.Commands.DeleteProject;
using Projects.Application.Services;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.DeleteTags
{
	public class DeleteTagsCommandHandler : IRequestHandler<DeleteTagsCommand, Result>
	{
		private readonly IProjectRepository _projectRepository;
		private readonly IProjectQueryService _queryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<AddTagsCommandHandler> _logger;
		private readonly TagParserService _tagParserService;

		public DeleteTagsCommandHandler(
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

		public async Task<Result> Handle(DeleteTagsCommand request, CancellationToken ct)
		{
			_logger.LogInformation("Handling DeleteTagsCommand for ProjectId {ProjectId}", request.ProjectId);

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
					project.DeleteTag(tag);
				}

				await _projectRepository.UpdateTagsAsync(request.ProjectId, request.Tags, ct);
				await _unitOfWork.SaveChangesAsync(ct);
				_logger.LogInformation("Tags [{Tags}] deleted from Project {ProjectId}", string.Join(", ", tagsResult.Value), project.Id);

				return Result.Ok();
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain exception while removing from project {ProjectId}", project.Id);
				return Result.Fail(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while removing tags from project {ProjectId}", project.Id);
				return Result.Fail("Unexpected error.");
			}
		}
	}
}
