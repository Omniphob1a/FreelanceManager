namespace Tasks.Api.GraphQL.Payloads
{
	public class CreateProjectTaskPayload
	{
		public bool Success { get; }
		public Guid? TaskId { get; }
		public string[] Errors { get; }

		private CreateProjectTaskPayload(bool success, Guid? taskId, string[] errors)
		{
			Success = success;
			TaskId = taskId;
			Errors = errors ?? Array.Empty<string>();
		}

		public static CreateProjectTaskPayload CreateSuccess(Guid id) => new(true, id, Array.Empty<string>());
		public static CreateProjectTaskPayload CreateFailure(string[] errors) => new(false, null, errors ?? Array.Empty<string>());
	}
}
