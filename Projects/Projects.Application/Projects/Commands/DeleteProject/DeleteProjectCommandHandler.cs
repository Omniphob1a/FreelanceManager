using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.DeleteProject
{
	public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result>
	{
		private readonly IProjectRepository _repository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<DeleteProjectCommandHandler> _logger;
		private readonly IProjectQueryService _projectQueryService;

		public DeleteProjectCommandHandler(IProjectRepository repository, IUnitOfWork unitOfWork, ILogger<DeleteProjectCommandHandler> logger, IProjectQueryService projectQueryService)
		{
			_repository = repository;
			_unitOfWork = unitOfWork;
			_logger = logger;
			_projectQueryService = projectQueryService;
		}

		public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var project = await _projectQueryService.GetByIdAsync(request.Id, cancellationToken);
				if (project is null)
					return Result.Fail("Project not found.");

				_logger.LogInformation("Trying to delete project with id: {Id}", request.Id);

				project.Delete();

				await _repository.DeleteAsync(request.Id, cancellationToken);
			    _unitOfWork.TrackEntity(project);	
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return Result.Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete project with ID {Id}", request.Id);
				return Result.Fail("Unable to delete the project.");
			}
		}
	}
}

