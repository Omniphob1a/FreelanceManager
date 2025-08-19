using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Domain.Aggregate.Enums
{
	public enum ProjectTaskStatus
	{
		ToDo = 0,
		InProgress = 1,
		Completed = 2,
		Cancelled = 3,
	}
}
