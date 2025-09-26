using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common;
using Tasks.Application.Common.Filters;
using Tasks.Application.Common.Pagination;
using Tasks.Persistence.Models;

namespace Tasks.Persistence.Extensions
{
	public static class TaskQueryExtension
	{
		public static IQueryable<ProjectTaskEntity> ApplyFilters(this IQueryable<ProjectTaskEntity> query, TaskFilter filter)
		{
			if (filter.IncludeComments)
				query = query.Include(t => t.Comments);

			if (filter.IncludeTimeEntries)
				query = query.Include(t => t.TimeEntries);

			if (filter.OnlyMyTasks && filter.CurrentUserId.HasValue)
			{
				var userId = filter.CurrentUserId.Value;
				query = query.Where(t => t.AssigneeId == userId || t.ReporterId == userId);
			}
			else
			{
				if (filter.AssigneeId.HasValue)
					query = query.Where(t => t.AssigneeId == filter.AssigneeId);

				if (filter.ReporterId.HasValue)
					query = query.Where(t => t.ReporterId == filter.ReporterId);
			}

			if (filter.ProjectId.HasValue)
				query = query.Where(t => t.ProjectId == filter.ProjectId);

			if (!string.IsNullOrWhiteSpace(filter.Search))
				query = query.Where(t => t.Title.Contains(filter.Search) || (t.Description != null && t.Description.Contains(filter.Search)));

			if (filter.Status.HasValue)
				query = query.Where(t => t.Status == filter.Status);

			if (filter.Priority.HasValue)
				query = query.Where(t => t.Priority == filter.Priority);

			if (filter.MinEstimatedHours.HasValue)
			{
				var minTicks = TimeSpan.FromHours((double)filter.MinEstimatedHours).Ticks;
				query = query.Where(t => t.TimeEstimatedTicks >= minTicks);
			}

			if (filter.MaxEstimatedHours.HasValue)
			{
				var maxTicks = TimeSpan.FromHours((double)filter.MaxEstimatedHours).Ticks;
				query = query.Where(t => t.TimeEstimatedTicks <= maxTicks);
			}

			if (filter.DueFrom.HasValue)
				query = query.Where(t => t.DueDate >= filter.DueFrom);

			if (filter.DueTo.HasValue)
				query = query.Where(t => t.DueDate <= filter.DueTo);

			if (filter.CreatedFrom.HasValue)
				query = query.Where(t => t.CreatedAt >= filter.CreatedFrom);

			if (filter.CreatedTo.HasValue)
				query = query.Where(t => t.CreatedAt <= filter.CreatedTo);

			if (filter.UpdatedFrom.HasValue)
				query = query.Where(t => t.UpdatedAt >= filter.UpdatedFrom);

			if (filter.UpdatedTo.HasValue)
				query = query.Where(t => t.UpdatedAt <= filter.UpdatedTo);

			if (filter.IsBillable.HasValue)
				query = query.Where(t => t.IsBillable == filter.IsBillable);

			if (filter.HasTimeEntries == true)
				query = query.Where(t => t.TimeEntries.Any());

			if (filter.HasComments == true)
				query = query.Where(t => t.Comments.Any());

			if (filter.Overdue == true)
				query = query.Where(t => t.DueDate < DateTime.UtcNow);

			query = ApplySorting(query, filter.SortBy, filter.Desc);

			return query;
		}

		public static IQueryable<ProjectTaskEntity> ApplyPagination(this IQueryable<ProjectTaskEntity> query, PaginationInfo paginationInfo)
		{
			var page = paginationInfo.ActualPage;
			if (page < 1) page = 1;

			var pageSize = paginationInfo.ItemsPerPage;
			if (pageSize < 1) pageSize = 1;


			query = query
				.Skip((page - 1) * pageSize)
				.Take(pageSize);

			return query;
		}

		private static IQueryable<ProjectTaskEntity> ApplySorting(this IQueryable<ProjectTaskEntity> query, string? sortBy, bool desc)
		{
			return (sortBy?.ToLowerInvariant()) switch
			{
				"created" => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
				"updated" => desc ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
				"duedate" => desc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
				"title" => desc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
				"priority" => desc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
				_ => query.OrderByDescending(t => t.CreatedAt),
			};
		}
	}
}
