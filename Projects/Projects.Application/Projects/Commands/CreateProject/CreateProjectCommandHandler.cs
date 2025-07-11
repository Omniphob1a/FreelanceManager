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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.CreateProject
{
	public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
	{
		private readonly IProjectRepository _repository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<CreateProjectCommandHandler> _logger;
		private readonly TagParserService _tagParserService;
		public CreateProjectCommandHandler(IProjectRepository repository, IUnitOfWork unitOfWork, ILogger<CreateProjectCommandHandler> logger, TagParserService tagParserService)
		{
			_repository = repository;
			_unitOfWork = unitOfWork;
			_logger = logger;
			_tagParserService = tagParserService;
		}

		public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Creating new project. Title: {Title}, OwnerId: {OwnerId}", request.Title, request.OwnerId);

			var tagsResult = _tagParserService.ParseTags(request.Tags);
			if (tagsResult.IsFailed)
				return Result.Fail<Guid>(tagsResult.Errors);

			Project project;
			try
			{
				project = Project.CreateDraft(
					request.Title,
					request.Description,
					request.OwnerId,
					new Budget(request.BudgetMin, request.BudgetMax, CurrencyCode.From(request.CurrencyCode)),
					Category.From(request.Category),
					tagsResult.Value
				);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while creating project. Title: {Title}, OwnerId: {OwnerId}", request.Title, request.OwnerId);
				return Result.Fail<Guid>(ex.Message);
			}

			try
			{
				await _repository.AddAsync(project, cancellationToken);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while saving project. Title: {Title}, OwnerId: {OwnerId}", request.Title, request.OwnerId);
				throw; 
			}

			_logger.LogInformation("Project created successfully with ID: {ProjectId}", project.Id);
			return Result.Ok(project.Id);
		}

	}
}
