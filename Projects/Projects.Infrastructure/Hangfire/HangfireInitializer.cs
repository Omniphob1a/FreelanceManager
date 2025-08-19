using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Interfaces;
using Projects.Application.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Hangfire
{
	public static class HangfireInitializer
	{
		public static void InitializeRecurringJobs(IBackgroundJobManager jobManager)
		{
			jobManager.AddOrUpdateRecurring<ArchiveOutOfDateProjectsJob>(
				recurringJobId: "archive-out-of-date-projects",
				methodCall: job => job.ExecuteAsync(),
				cronExpression: "*/5 * * * *" 
			);

			jobManager.AddOrUpdateRecurring<EscalateOutOfDateMilestonesJob>(
				recurringJobId: "escalate-out-of-date-milestones",
				methodCall: job => job.ExecuteAsync(),
				cronExpression: "*/5 * * * *"
			);
		}
	}
}
