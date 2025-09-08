using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.ProjectTasks.Commands.AddComment
{
	public record AddCommentCommand(Guid TaskId, Guid AuthorId, string Text) : IRequest<Result<Unit>>;
}
