using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.DeleteAttachment
{
	public record DeleteAttachmentCommand(Guid ProjectId, Guid AttachmentId) : IRequest<Result>;
}
