using Projects.Domain.Enums;
using Projects.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Filters
{
	public class ProjectFilter
	{
		public Guid? OwnerId { get; set; }
		public ProjectStatus? Status { get; set; }

		public string? Category { get; set; } 

		public decimal? MinBudget { get; set; }
		public decimal? MaxBudget { get; set; }

		public DateTime? CreatedFrom { get; set; }
		public DateTime? CreatedTo { get; set; }

		public DateTime? ExpiresFrom { get; set; }
		public DateTime? ExpiresTo { get; set; }

		public List<string>? TagNames { get; set; }

		public string? Search { get; set; }

		public string? SortBy { get; set; } = "created";
		public bool Desc { get; set; } = true;

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;

		public string ToCacheKey()
		{
			var builder = new StringBuilder();

			builder.Append($"owner={OwnerId?.ToString() ?? "null"}|");
			builder.Append($"status={Status?.ToString() ?? "null"}|");
			builder.Append($"category={Category ?? "null"}|");
			builder.Append($"minBudget={MinBudget?.ToString("F2") ?? "null"}|");
			builder.Append($"maxBudget={MaxBudget?.ToString("F2") ?? "null"}|");
			builder.Append($"createdFrom={CreatedFrom?.ToString("O") ?? "null"}|");
			builder.Append($"createdTo={CreatedTo?.ToString("O") ?? "null"}|");
			builder.Append($"expiresFrom={ExpiresFrom?.ToString("O") ?? "null"}|");
			builder.Append($"expiresTo={ExpiresTo?.ToString("O") ?? "null"}|");

			var tags = TagNames is { Count: > 0 } ? string.Join(",", TagNames.OrderBy(x => x)) : "null";
			builder.Append($"tags={tags}|");

			builder.Append($"search={Search ?? "null"}|");
			builder.Append($"sortBy={SortBy ?? "created"}|");
			builder.Append($"desc={(Desc ? "1" : "0")}|");
			builder.Append($"page={Page}|");
			builder.Append($"size={PageSize}");

			return builder.ToString();
		}

	}
}
