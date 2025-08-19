using FluentResults;
using MediatR;
using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetFullProjectById
{
	public record GetFullProjectByIdQuery(Guid ProjectId) : IRequest<Result<ProjectDto>>;
}
