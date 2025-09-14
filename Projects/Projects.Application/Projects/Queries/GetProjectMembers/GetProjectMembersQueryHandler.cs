using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;

namespace Projects.Application.Projects.Queries.GetProjectMembers
{
	public class GetProjectMembersQueryHandler
		: IRequestHandler<GetProjectMembersQuery, Result<List<ProjectMemberReadDto>>>
	{
		private readonly IProjectMemberQueryService _membersQueryService;
		private readonly ILogger<GetProjectMembersQueryHandler> _logger;

		public GetProjectMembersQueryHandler(
			IProjectMemberQueryService membersQueryService,
			ILogger<GetProjectMembersQueryHandler> logger)
		{
			_membersQueryService = membersQueryService;
			_logger = logger;
		}

		public async Task<Result<List<ProjectMemberReadDto>>> Handle(
			GetProjectMembersQuery request,
			CancellationToken ct)
		{
			_logger.LogDebug("Handling GetProjectMembersQuery for ProjectId: {ProjectId}", request.ProjectId);

			try
			{
				var members = await _membersQueryService.GetProjectMembersAsync(request.ProjectId, ct);
				return Result.Ok(members);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching members for project {ProjectId}", request.ProjectId);
				return Result.Fail<List<ProjectMemberReadDto>>("Unexpected error occurred.");
			}
		}
	}
}
