using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetFullProjectById
{
	public class GetFullProjectByIdQueryHandler : IRequestHandler<GetFullProjectByIdQuery, Result<ProjectDto>>
	{
		private readonly IProjectQueryService _queryService;
		private readonly IMapper _mapper;
		private readonly ILogger<GetFullProjectByIdQueryHandler> _logger;

		public GetFullProjectByIdQueryHandler(
			IProjectQueryService queryService,
			IMapper mapper,
			ILogger<GetFullProjectByIdQueryHandler> logger)
		{
			_queryService = queryService;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<Result<ProjectDto>> Handle(GetFullProjectByIdQuery request, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Handling GetFullProjectByIdQuery for Id: {ProjectId}", request.ProjectId);

			try
			{
				var project = await _queryService.GetFullProjectByIdAsync(request.ProjectId, cancellationToken);

				if (project is null)
				{
					_logger.LogWarning("Project with Id {ProjectId} not found", request.ProjectId);
					return Result.Fail<ProjectDto>($"Project with Id {request.ProjectId} not found");
				}

				var dto = _mapper.Map<ProjectDto>(project);
				_logger.LogInformation("Project with Id {ProjectId} retrieved successfully", request.ProjectId);


				return Result.Ok(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving full project with Id: {ProjectId}", request.ProjectId);
				return Result.Fail<ProjectDto>("Unexpected error occurred while retrieving the full project");
			}
		}
	}
}
