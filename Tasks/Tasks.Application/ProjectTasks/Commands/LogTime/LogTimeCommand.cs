using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.AddTimeEntry
{
	public record LogTimeCommand(
		Guid TaskId,
		Guid UserId,
		DateTime StartedAt,
		DateTime EndedAt,
		string? Description,
		bool IsBillable,
		decimal? HourlyRate,
		string? Currency
		) : IRequest<Result<Unit>>;
}
