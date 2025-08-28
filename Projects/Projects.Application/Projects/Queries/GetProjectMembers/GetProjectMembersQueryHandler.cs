using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetProjectMembers
{
	public class GetProjectMembersQueryHandler
	: IRequestHandler<GetProjectMembersQuery, Result<List<ProjectMemberDto>>>
	{
		private readonly IProjectQueryService _queryService;
		private readonly ILogger<GetProjectMembersQueryHandler> _logger;

		public GetProjectMembersQueryHandler(
			IProjectQueryService queryService,
			ILogger<GetProjectMembersQueryHandler> logger)
		{
			_queryService = queryService;
			_logger = logger;
		}

		public async Task<Result<List<ProjectMemberDto>>> Handle(
			GetProjectMembersQuery request,
			CancellationToken ct)
		{
			_logger.LogDebug("Handling GetProjectMembersQuery for ProjectId: {ProjectId}", request.ProjectId);

			try
			{
				var members = await _queryService.GetMembersAsync(request.ProjectId, ct);

				if (members is null || !members.Any())
				{
					_logger.LogInformation("No members found for project {ProjectId}", request.ProjectId);
					return Result.Ok(new List<ProjectMemberDto>()); 
				}

				return Result.Ok(members);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching members for project {ProjectId}", request.ProjectId);
				return Result.Fail<List<ProjectMemberDto>>("Unexpected error occurred.");
			}
		}
	}
}
