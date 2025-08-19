using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.AddTimeEntry
{
	public record AddTimeEntryCommand(
		Guid TaskId,
		Guid UserId,
		DateTime Start,
		DateTime End,
		string? Description,
		bool IsBillable,
		decimal? HourlyRate,
		string? Currency
		) : IRequest<Result<Unit>>;
}
