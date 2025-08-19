using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.Common.Filters
{
	public class TaskFilter
	{
		public Guid? ProjectId { get; set; }
		public Guid? AssigneeId { get; set; }
		public Guid? ReporterId { get; set; }
		public Guid? CreatedBy { get; set; }
		public string? Search { get; set; }
		public int? Status { get; set; }
		public int? Priority { get; set; }
		public decimal? MinEstimatedHours { get; set; }
		public decimal? MaxEstimatedHours { get; set; }
		public decimal? MinSpentHours { get; set; }
		public decimal? MaxSpentHours { get; set; }
		public DateTime? DueFrom { get; set; }
		public DateTime? DueTo { get; set; }
		public DateTime? CreatedFrom { get; set; }
		public DateTime? CreatedTo { get; set; }
		public DateTime? UpdatedFrom { get; set; }
		public DateTime? UpdatedTo { get; set; }
		public bool? IsBillable { get; set; }
		public bool? HasTimeEntries { get; set; }
		public bool? HasComments { get; set; }
		public bool? Overdue { get; set; }
		public string? SortBy { get; set; } = "created";
		public bool Desc { get; set; } = true;
		public bool IncludeTimeEntries { get; set; } = false;
		public bool IncludeComments { get; set; } = false;

		public string ToCacheKey()
		{
			var builder = new StringBuilder();

			builder.Append($"project={ProjectId?.ToString() ?? "null"}|");
			builder.Append($"assignee={AssigneeId?.ToString() ?? "null"}|");
			builder.Append($"reporter={ReporterId?.ToString() ?? "null"}|");
			builder.Append($"createdBy={CreatedBy?.ToString() ?? "null"}|");

			builder.Append($"status={Status?.ToString() ?? "null"}|");
			builder.Append($"priority={Priority?.ToString() ?? "null"}|");

			builder.Append($"minEstimatedHours={MinEstimatedHours?.ToString("F2") ?? "null"}|");
			builder.Append($"maxEstimatedHours={MaxEstimatedHours?.ToString("F2") ?? "null"}|");

			builder.Append($"minSpentHours={MinSpentHours?.ToString("F2") ?? "null"}|");
			builder.Append($"maxSpentHours={MaxSpentHours?.ToString("F2") ?? "null"}|");

			builder.Append($"dueFrom={DueFrom?.ToString("O") ?? "null"}|");
			builder.Append($"dueTo={DueTo?.ToString("O") ?? "null"}|");

			builder.Append($"createdFrom={CreatedFrom?.ToString("O") ?? "null"}|");
			builder.Append($"createdTo={CreatedTo?.ToString("O") ?? "null"}|");

			builder.Append($"updatedFrom={UpdatedFrom?.ToString("O") ?? "null"}|");
			builder.Append($"updatedTo={UpdatedTo?.ToString("O") ?? "null"}|");

			var hasTimeEntries = HasTimeEntries.HasValue ? (HasTimeEntries.Value ? "1" : "0") : "null";
			var hasComments = HasComments.HasValue ? (HasComments.Value ? "1" : "0") : "null";
			var isBillable = IsBillable.HasValue ? (IsBillable.Value ? "1" : "0") : "null";
			var overdue = Overdue.HasValue ? (Overdue.Value ? "1" : "0") : "null";

			builder.Append($"isBillable={isBillable}|");
			builder.Append($"hasTimeEntries={hasTimeEntries}|");
			builder.Append($"hasComments={hasComments}|");
			builder.Append($"overdue={overdue}|");

			builder.Append($"search={Search ?? "null"}|");
			builder.Append($"sortBy={SortBy ?? "created"}|");
			builder.Append($"desc={(Desc ? "1" : "0")}|");

			builder.Append($"includeTimeEntries={(IncludeTimeEntries ? "1" : "0")}|");
			builder.Append($"includeComments={(IncludeComments ? "1" : "0")}");

			return builder.ToString();
		}

	}
}
