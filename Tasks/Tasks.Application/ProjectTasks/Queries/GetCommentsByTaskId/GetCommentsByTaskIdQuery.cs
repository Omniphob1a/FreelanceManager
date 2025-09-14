using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;

namespace Tasks.Application.ProjectTasks.Queries.GetComments
{
	public record GetCommentsByTaskIdQuery(Guid TaskId) : IRequest<Result<List<CommentReadDto>>>;
}
