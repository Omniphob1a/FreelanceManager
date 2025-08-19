using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Interfaces
{
	public interface IBackgroundJobManager
	{
		string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall) where TJob : class; // Асинхронная версия

		string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay) where TJob : class;

		void AddOrUpdateRecurring<TJob>(string recurringJobId, Expression<Func<TJob, Task>> methodCall, string cronExpression);

		string ContinueWith(string parentJobId, Expression<Action> continuation);
	}
}
