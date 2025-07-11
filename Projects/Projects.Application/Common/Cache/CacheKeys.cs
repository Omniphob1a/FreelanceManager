using Projects.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Cache
{
	public static class CacheKeys
	{
		public const string ProjectListAll = "projects:all";
		public const string ProjectListActive = "projects:active";
		public const string ProjectListCompleted = "projects:completed";
		public const string FilteredProjectListPrefix = "project:list:filtered:";
		public static string Project(Guid id) => $"project:{id}";
		public static string DraftProjects(Guid ownerId) =>	$"projects:drafts:{ownerId}";
	}
}
