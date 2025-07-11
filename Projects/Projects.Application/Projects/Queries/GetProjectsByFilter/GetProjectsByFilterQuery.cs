using FluentResults;
using MediatR;
using Projects.Application.Common.Abstractions;
using Projects.Application.Common.Filters;
using Projects.Application.Common.Pagination;
using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Projects.Application.Projects.Queries.GetProjectsByFilter
{
	public record GetProjectsByFilterQuery(ProjectFilter Filter) : IRequest<Result<PaginatedResult<ProjectDto>>>,
		ICacheableQuery
	{
		public string CacheKey => $"project:list:filtered:{Filter.ToCacheKey()}";
		public TimeSpan Ttl => TimeSpan.FromMinutes(2);
	}
}
