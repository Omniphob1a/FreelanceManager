using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.CreateProject
{
	public record CreateProjectCommand(
		string Title,
		string Description,
		Guid OwnerId,
		decimal? BudgetMin,
		decimal? BudgetMax,
		string Currency,
		string Category,
		List<string> Tags
	) : IRequest<Result<Guid>>;
}
