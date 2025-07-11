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

		public DeleteProjectCommandHandler(IProjectRepository repository, IUnitOfWork unitOfWork, ILogger<DeleteProjectCommandHandler> logger)
		{
			_repository = repository;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Trying to delete project with id: {Id}", request.Id);
				await _repository.DeleteAsync(request.Id, cancellationToken);
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

