using Ardalis.Specification;
using Projects.Application.Common.Filters;
using Projects.Domain.Entities;
using Projects.Persistence.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Specifications
{
	public class ProjectsByFilterSpec : Specification<ProjectEntity>
	{
		public ProjectsByFilterSpec(ProjectFilter filter, bool includePaging = false)
		{
			if (filter.IncludeMilestones)
				Query.Include(p => p.Milestones);
			if (filter.IncludeAttachments)
				Query.Include(p => p.Attachments);

			if (filter.Status.HasValue)
				Query.Where(p => p.Status == (int)filter.Status);

			if (filter.OwnerId.HasValue)
				Query.Where(p => p.OwnerId == filter.OwnerId);

			if (!string.IsNullOrWhiteSpace(filter.Category))
			{
				var normalizedCategory = filter.Category.Trim().ToLowerInvariant();
				Query.Where(p => p.Category.ToLower() == normalizedCategory);
			}

			if (filter.MinBudget.HasValue)
				Query.Where(p => p.BudgetMin.HasValue && p.BudgetMin >= filter.MinBudget);

			if (filter.MaxBudget.HasValue)
				Query.Where(p => p.BudgetMax.HasValue && p.BudgetMax <= filter.MaxBudget);

			if (filter.CreatedFrom.HasValue)
				Query.Where(p => p.CreatedAt >= filter.CreatedFrom);

			if (filter.CreatedTo.HasValue)
				Query.Where(p => p.CreatedAt <= filter.CreatedTo);

			if (filter.ExpiresFrom.HasValue)
				Query.Where(p => p.ExpiresAt.HasValue && p.ExpiresAt >= filter.ExpiresFrom);

			if (filter.ExpiresTo.HasValue)
				Query.Where(p => p.ExpiresAt.HasValue && p.ExpiresAt <= filter.ExpiresTo);

			if (filter.TagNames?.Any() == true)
			{
				var normalizedTags = filter.TagNames
					.Select(t => t.Trim().ToLowerInvariant())
					.ToList();

				Query.Where(p =>
					normalizedTags.Any(tag =>
						p.Tags.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
							.Contains(tag)));
			}

			if (!string.IsNullOrWhiteSpace(filter.Search))
				Query.Where(p =>
					p.Title.Contains(filter.Search) ||
					p.Description.Contains(filter.Search));

			switch (filter.SortBy?.ToLowerInvariant())
			{
				case "created":
					Query.OrderByDescending(p => p.CreatedAt);
					break;
				case "expires":
					Query.OrderByDescending(p => p.ExpiresAt);
					break;
				case "budgetmin":
					Query.OrderByDescending(p => p.BudgetMin);
					break;
				case "budgetmax":
					Query.OrderByDescending(p => p.BudgetMax);
					break;
				case "title":
					Query.OrderByDescending(p => p.Title);
					break;
				default:
					Query.OrderByDescending(p => p.CreatedAt);
					break;
			}

			if (includePaging)
			{
				Query.Skip((filter.Page - 1) * filter.PageSize)
					 .Take(filter.PageSize);
			}
		}
	}

}
