using FluentResults;
using MediatR;
using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetProjectById
{
	public record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;
}
