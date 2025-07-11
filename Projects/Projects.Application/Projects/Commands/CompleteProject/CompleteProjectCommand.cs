using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.CompleteProject
{
	public record CompleteProjectCommand(Guid ProjectId) : IRequest<Result>;
}
