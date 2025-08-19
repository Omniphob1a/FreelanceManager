using Hangfire;
using Projects.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Hangfire
{
	public class BackgroundJobManager : IBackgroundJobManager
	{
		public void AddOrUpdateRecurring<TJob>(string recurringJobId, Expression<Func<TJob, Task>> methodCall, string cronExpression)
			=> RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);

		public string ContinueWith(string parentJobId, Expression<Action> continuation)
		{
			throw new NotImplementedException();
		}

		public string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall) where TJob : class
		{
			throw new NotImplementedException();
		}

		public string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay) where TJob : class
		{
			throw new NotImplementedException();
		}
	}
}
