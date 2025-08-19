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
		string Description,
		decimal EstimateValue,
		int EstimateUnit,
		DateTime DueDate,
		bool IsBillable,
		decimal Amount,
		string Currency) 
		: IRequest<Result<Unit>>;
}
