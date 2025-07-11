using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Domain.Entities;

namespace Projects.Application.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
	private readonly IProjectQueryService _queryService;
	private readonly IMapper _mapper;
	private readonly ILogger<GetProjectByIdQueryHandler> _logger;

	public GetProjectByIdQueryHandler(
		IProjectQueryService queryService,
		IMapper mapper,
		ILogger<GetProjectByIdQueryHandler> logger)
	{
		_queryService = queryService;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
	{
		_logger.LogDebug("Handling GetProjectByIdQuery for Id: {ProjectId}", request.Id);

		try
		{
			var project = await _queryService.GetByIdAsync(request.Id, cancellationToken);

			if (project is null)
			{
				_logger.LogWarning("Project with Id {ProjectId} not found", request.Id);
				return Result.Fail<ProjectDto>($"Project with Id {request.Id} not found");
			}

			var dto = _mapper.Map<ProjectDto>(project);
			_logger.LogInformation("Project with Id {ProjectId} retrieved successfully", request.Id);

			return Result.Ok(dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while retrieving project with Id: {ProjectId}", request.Id);
			return Result.Fail<ProjectDto>("Unexpected error occurred while retrieving the project");
		}
	}
}
