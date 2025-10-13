namespace Tasks.Api.GraphQL.Inputs
{
	public record CreateProjectTaskInput(
		Guid ProjectId,
		string Title,
		string? Description,
		Guid ReporterId,
		Guid CreatedBy,
		bool IsBillable,
		int Priority,
		Guid? AssigneeId,
		int? TimeEstimatedMinutes,
		DateTime? DueDate,
		decimal? HourlyRate,
		string? Currency
	);
}
