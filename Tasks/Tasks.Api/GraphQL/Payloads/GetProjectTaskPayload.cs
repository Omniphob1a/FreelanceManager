using Tasks.Application.DTOs;

namespace Tasks.Api.GraphQL.Payloads
{
	public class GetProjectTaskPayload
	{
		public bool Success { get; }
		public ProjectTaskDto? Task { get; }
		public string[] Errors { get; }

		private GetProjectTaskPayload(bool success, ProjectTaskDto? task, string[] errors)
		{
			Success = success;
			Task = task;
			Errors = errors ?? Array.Empty<string>();
		}

		public static GetProjectTaskPayload CreateSuccess(ProjectTaskDto dto) => new(true, dto, Array.Empty<string>());
		public static GetProjectTaskPayload CreateFailure(string[] errors) => new(false, null, errors ?? Array.Empty<string>());
	}
}
