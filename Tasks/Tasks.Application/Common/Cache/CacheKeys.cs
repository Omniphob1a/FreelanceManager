using System;

namespace Tasks.Application.Common.Cache
{
	public static class CacheKeys
	{
		public const string TaskListAll = "tasks:all";
		public const string TaskListActive = "tasks:active";
		public const string TaskListCompleted = "tasks:completed";

		public static string TaskListByProject(Guid projectId) => $"tasks:project:{projectId}";

		public static string Task(Guid taskId) => $"task:{taskId}";

		public static string TasksByAssignee(Guid userId) => $"tasks:assignee:{userId}";

		public static string FilteredTaskListPrefix = "tasks:list:filtered:";
	}
}
