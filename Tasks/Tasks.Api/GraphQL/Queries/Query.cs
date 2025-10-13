using MediatR;
using Tasks.Api.GraphQL.Payloads;
using Tasks.Application.Common;
using Tasks.Application.ProjectTasks.Queries.GetProjectMembers;
using Tasks.Application.ProjectTasks.Queries.GetProjectTaskById;

namespace Tasks.Api.GraphQL.Queries
{
	public class Query
	{
		// УБРАЛИ конструктор с IMediator!

		public async Task<GetProjectTaskPayload> ProjectTaskByIdAsync(
			Guid id,
			[Service] IMediator mediator)  // ← ИНЖЕКТИМ В МЕТОД
		{
			var result = await mediator.Send(new GetProjectTaskByIdQuery(id, Enumerable.Empty<TaskIncludeOptions>()));
			if (result.IsFailed)
				return GetProjectTaskPayload.CreateFailure(result.Errors.Select(e => e.Message).ToArray());
			return GetProjectTaskPayload.CreateSuccess(result.Value);
		}

		public async Task<GetProjectMembersPayload> ProjectMembersAsync(
			Guid projectId,
			[Service] IMediator mediator)  // ← ИНЖЕКТИМ В МЕТОД
		{
			var result = await mediator.Send(new GetProjectMembersQuery(projectId));
			if (result.IsFailed)
				return GetProjectMembersPayload.CreateFailure(result.Errors.Select(e => e.Message).ToArray());
			return GetProjectMembersPayload.CreateSuccess(result.Value);
		}
	}
}