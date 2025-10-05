using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.UpdateProjectTask
{
	public record UpdateProjectTaskCommand(
		Guid TaskId,
		string Title,
		string? Description,
		TimeSpan? TimeEstimated,
		DateTime? DueDate,
		bool IsBillable,
		decimal? HourlyRate,
		string? Currency,
		int Priority,
		Guid? AssigneeId
	) : IRequest<Result<Unit>>;

}
