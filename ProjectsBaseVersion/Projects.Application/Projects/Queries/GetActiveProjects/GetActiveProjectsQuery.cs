using FluentResults;
using MediatR;
using Projects.Application.DTOs;
using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetActiveProjects
{
	public record GetActiveProjectsQuery() : IRequest<Result<IEnumerable<ProjectDto>>>;
}
