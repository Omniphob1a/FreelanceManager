using System;

namespace Tasks.Application.Common.Cache
{
	public static class CacheKeys
	{
		public const string TaskListAll = "task:list:all";
		public const string TaskListActive = "task:list:active";
		public const string TaskListCompleted = "task:list:completed";

		public static string TaskListByProject(Guid projectId) => $"task:list:project:{projectId}";

		public static string Task(Guid taskId) => $"task:{taskId}";

		public static string TasksByAssignee(Guid userId) => $"task:list:assignee:{userId}";

		public const string FilteredTaskListPrefix = "task:list:filtered:";
	}
}
