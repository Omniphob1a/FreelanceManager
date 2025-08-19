using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Application.Services;
using Projects.Domain.Entities;
using Projects.Domain.Exceptions;
using Projects.Domain.Repositories;
using Projects.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.UpdateProject
{
	public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result>
	{
		private readonly IProjectRepository _repository;
		private readonly IProjectQueryService _queryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<UpdateProjectCommandHandler> _logger;
		private readonly TagParserService _tagParserService;

		public UpdateProjectCommandHandler(IProjectQueryService queryService,
			IProjectRepository repository,
			IUnitOfWork unitOfWork,
			ILogger<UpdateProjectCommandHandler> logger,
			TagParserService tagParserService)
		{
			_queryService = queryService;
			_unitOfWork = unitOfWork;
			_repository = repository;
			_logger = logger;
			_tagParserService = tagParserService;
		}

		public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken ct)
		{
			_logger.LogDebug("Starting update of project with Id: {ProjectId}", request.ProjectId);

			var tagsResult = _tagParserService.ParseTags(request.Tags);
			if (tagsResult.IsFailed)
			{
				_logger.LogWarning("Tag parsing failed for ProjectId {ProjectId} with errors: {Errors}", request.ProjectId, tagsResult.Errors);
				return Result.Fail(tagsResult.Errors);
			}

			Project? project;
			try
			{
				project = await _queryService.GetByIdAsync(request.ProjectId, ct);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error fetching project {ProjectId} for update", request.ProjectId);
				throw;
			}

			if (project is null)
			{
				_logger.LogWarning("Project not found for ID {ProjectId}", request.ProjectId);
				return Result.Fail("Project not found");
			}

			try
			{
				project.UpdateDetails(
					request.Title,
					request.Description,
					new Budget(request.BudgetMin, request.BudgetMax, CurrencyCode.From(request.CurrencyCode)),
					Category.From(request.Category),
					tagsResult.Value);
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Validation error while updating project {ProjectId}", request.ProjectId);
				return Result.Fail(ex.Message);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while updating project {ProjectId}", request.ProjectId);
				return Result.Fail(ex.Message);
			}

			try
			{
				await _repository.UpdateAsync(project, ct);
				_unitOfWork.TrackEntity(project);
				await _unitOfWork.SaveChangesAsync(ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error saving updated project {ProjectId}", request.ProjectId);
				throw;
			}

			_logger.LogInformation("Project {ProjectId} updated successfully", request.ProjectId);
			return Result.Ok();
		}
	}
}
