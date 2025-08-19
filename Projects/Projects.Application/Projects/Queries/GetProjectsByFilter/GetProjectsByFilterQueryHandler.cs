using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Common.Pagination;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Application.Projects.Queries.GetProjectById;
using Projects.Domain.Entities;
using Projects.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetProjectsByFilter
{
	public class GetProjectsByFilterQueryHandler : IRequestHandler<GetProjectsByFilterQuery, Result<PaginatedResult<ProjectDto>>>
	{
		private readonly IProjectQueryService _queryService;
		private readonly ILogger<GetProjectByIdQueryHandler> _logger;
		private readonly IMapper _mapper;

		public GetProjectsByFilterQueryHandler(IProjectQueryService queryService, ILogger<GetProjectByIdQueryHandler> logger, IMapper mapper)
		{
			_logger = logger;
			_mapper = mapper;
			_queryService = queryService;
		}

		public async Task<Result<PaginatedResult<ProjectDto>>> Handle(GetProjectsByFilterQuery request, CancellationToken ct)
		{
			if (request.Filter == null)
			{
				_logger.LogWarning("Filter is null");
				return Result.Fail<PaginatedResult<ProjectDto>>("Filter cannot be null");
			}

			try
			{
				var paginatedProjects = await _queryService.GetPaginatedAsync(request.Filter, ct);

				var items = paginatedProjects.Items ?? new List<Project>();
				var dtos = _mapper.Map<List<ProjectDto>>(items) ?? new List<ProjectDto>();

				var paginatedResultDto = new PaginatedResult<ProjectDto>(
					dtos,
					paginatedProjects.Pagination.TotalItems,
					paginatedProjects.Pagination.ActualPage,
					paginatedProjects.Pagination.ItemsPerPage);

				_logger.LogInformation("Projects successfully retrieved, total: {Total}", paginatedResultDto.Pagination.TotalItems);
				return Result.Ok(paginatedResultDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving projects with filter {@Filter}", request.Filter);
				return Result.Fail<PaginatedResult<ProjectDto>>("Unexpected error occurred");
			}
		}
	}
}
