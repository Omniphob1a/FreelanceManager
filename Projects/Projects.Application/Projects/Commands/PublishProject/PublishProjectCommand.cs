using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.PublishProject
{
	public record PublishProjectCommand(Guid ProjectId, DateTime ExpiresAt)
	: IRequest<Result>;
}
