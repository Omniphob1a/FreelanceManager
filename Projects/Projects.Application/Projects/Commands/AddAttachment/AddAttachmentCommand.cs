using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Commands.AddAttachment
{
	public record AddAttachmentCommand(Guid ProjectId, IFormFile File) : IRequest<Result<ProjectAttachmentDto>>;
}
