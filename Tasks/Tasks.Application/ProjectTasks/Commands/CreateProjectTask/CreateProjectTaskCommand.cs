using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.ValueObjects;

namespace Tasks.Application.ProjectTasks.Commands.CreateProjectTask
{
	public record CreateProjectTaskCommand(
		Guid ProjectId,
		string Title,
		string? Description,
		Guid ReporterId,
		Guid CreatedBy,
		Guid? AssigneeId = null,
		WorkEstimate? Estimate = null,
		DateTime? DueDate = null,
		bool IsBillable = false,
		Money? HourlyRate = null
	) : IRequest<Result<Guid>>;
}
