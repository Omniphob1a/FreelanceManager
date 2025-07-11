using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
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
		public CreateProjectCommandHandler(IProjectRepository repository, IUnitOfWork unitOfWork, ILogger<CreateProjectCommandHandler> logger)
		{
			_repository = repository;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Creating new project. Title: {Title}, OwnerId: {OwnerId}", request.Title, request.OwnerId);

				var budget = new Budget(request.BudgetMin, request.BudgetMax, request.Currency);

				var project = Project.CreateDraft(
					request.Title,
					request.Description,
					request.OwnerId,
					budget,
					request.Category,
					request.Tags);

				await _repository.AddAsync(project, cancellationToken);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				_logger.LogInformation("Project created successfully with ID: {ProjectId}", project.Id);

				return Result.Ok(project.Id);
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Validation error while creating project. Title: {Title}, OwnerId: {OwnerId}", request.Title, request.OwnerId);
				return Result.Fail<Guid>(ex.Message);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while creating project. Title: {Title}, OwnerId: {OwnerId}", request.Title, request.OwnerId);
				return Result.Fail<Guid>(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while creating project. Title: {Title}, OwnerId: {OwnerId}", request.Title, request.OwnerId);
				return Result.Fail<Guid>("An unexpected error occurred while creating the project.");
			}
		}
	}
}
