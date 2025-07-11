using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Domain.Entities;
using Projects.Domain.Repositories;
using Projects.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetProjectById
{
	public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
	{
		private readonly IProjectRepository _projectRepository;
		private readonly ILogger<GetProjectByIdQueryHandler> _logger;
		private readonly IMapper _mapper;

		public GetProjectByIdQueryHandler(IProjectRepository projectRepository, ILogger<GetProjectByIdQueryHandler> logger, IMapper mapper) 
		{
			_projectRepository = projectRepository;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
		{
			var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

			if (project == null)
			{
				_logger.LogWarning("Project with id {ProjectId} not found", request.Id);
				return Result.Fail<ProjectDto>($"Project with id {request.Id} not found");
			}

			try
			{
				var projectDto = _mapper.Map<ProjectDto>(project);
				return Result.Ok(projectDto);
			}
			catch (Exception ex)
			{
				_logger.LogMappingError<Project, ProjectDto>(project, ex);
				throw;
			}
		}
	}
}
