using MediatR;
using Tasks.Api.GraphQL.Inputs;
using Tasks.Api.GraphQL.Payloads;
using Tasks.Application.ProjectTasks.Commands.CreateProjectTask;

namespace Tasks.Api.GraphQL.Mutations
{
	public class Mutation
	{
		private readonly IMediator _mediator;
		public Mutation(IMediator mediator) => _mediator = mediator;

		public async Task<CreateProjectTaskPayload> CreateProjectTaskAsync(CreateProjectTaskInput input)
		{
			var cmd = new CreateProjectTaskCommand(
				input.ProjectId,
				input.Title,
				input.Description,
				input.ReporterId,
				input.CreatedBy,
				input.IsBillable,
				input.Priority,
				input.AssigneeId,
				input.TimeEstimatedMinutes.HasValue ? TimeSpan.FromMinutes(input.TimeEstimatedMinutes.Value) : null,
				input.DueDate,
				input.HourlyRate,
				input.Currency
			);

			var result = await _mediator.Send(cmd);

			if (result.IsFailed)
				return CreateProjectTaskPayload.CreateFailure(result.Errors.Select(e => e.Message).ToArray());

			return CreateProjectTaskPayload.CreateSuccess(result.Value);
		}
	}
}
