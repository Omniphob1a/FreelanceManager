using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Enums.Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.ValueObjects;

namespace Tasks.Application.ProjectTasks.Commands.CreateProjectTask
{
	public record CreateProjectTaskCommand(
		Guid ProjectId,
		string Title,
		string? Description,
		Guid ReporterId,
		Guid CreatedBy,
		bool IsBillable,
		int Priority,
		Guid? AssigneeId = null,
		TimeSpan? TimeEstimated = null,
		DateTime? DueDate = null,
		decimal? HourlyRate = null,
		string? Currency = null
	) : IRequest<Result<Guid>>;
}
