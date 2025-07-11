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

namespace Projects.Application.Projects.Queries.GetActiveProjects
{
	public class GetActiveProjectsQueryHandler : IRequestHandler<GetActiveProjectsQuery, Result<IEnumerable<ProjectDto>>>
	{
		private readonly IProjectRepository _projectRepository;
		private readonly ILogger<GetActiveProjectsQueryHandler> _logger;
		private readonly IMapper _mapper;

		public GetActiveProjectsQueryHandler(IProjectRepository projectRepository, ILogger<GetActiveProjectsQueryHandler> logger, IMapper mapper)
		{
			_projectRepository = projectRepository;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<Result<IEnumerable<ProjectDto>>> Handle(GetActiveProjectsQuery request, CancellationToken cancellationToken)
		{
			var projects = await _projectRepository.GetActiveAsync(cancellationToken);

			if (!projects.Any())
			{
				_logger.LogWarning("Active projects not found");
				return Result.Fail<IEnumerable<ProjectDto>>("Active projects not found");
			}

			try
			{
				var projectDtos = _mapper.Map<List<ProjectDto>>(projects);
				return Result.Ok<IEnumerable<ProjectDto>>(projectDtos);
			}
			catch (Exception ex)
			{
				_logger.LogMappingError<Project, ProjectDto>(projects, ex);
				throw;
			}
		}
	}
}
