using Tasks.Application.DTOs;

namespace Tasks.Api.GraphQL.Payloads
{
	public class GetProjectMembersPayload
	{
		public bool Success { get; }
		public List<ProjectMemberReadDto>? Members { get; }
		public string[] Errors { get; }

		private GetProjectMembersPayload(bool success, List<ProjectMemberReadDto>? members, string[] errors)
		{
			Success = success;
			Members = members;
			Errors = errors ?? Array.Empty<string>();
		}

		public static GetProjectMembersPayload CreateSuccess(List<ProjectMemberReadDto> members) => new(true, members, Array.Empty<string>());
		public static GetProjectMembersPayload CreateFailure(string[] errors) => new(false, null, errors ?? Array.Empty<string>());
	}
}
