using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.AddTag
{
	public record AddTagsCommand(Guid ProjectId, List<string> Tags) : IRequest<Result>;
}
