using FluentResults;
using MediatR;
using Projects.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.DeleteTags
{
	public record DeleteTagsCommand(Guid ProjectId, List<string> Tags) : IRequest<Result>;

}
