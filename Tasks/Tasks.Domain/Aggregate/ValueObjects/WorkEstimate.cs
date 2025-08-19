using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Common;

namespace Tasks.Domain.Aggregate.ValueObjects
{
	public sealed class WorkEstimate : ValueObject
	{
		public decimal Value { get; }
		public WorkUnit Unit { get; }

		private WorkEstimate(decimal value, WorkUnit unit)
		{
			if (value <= 0)
				throw new ArgumentException("Estimate must be positive", nameof(value));

			Value = value;
			Unit = unit;
		}
		public TimeSpan ToTimeSpan()
		{
			return Unit switch
			{
				WorkUnit.Minutes => TimeSpan.FromMinutes((double)Value),
				WorkUnit.Hours => TimeSpan.FromHours((double)Value),
				WorkUnit.Days => TimeSpan.FromDays((double)Value),
				_ => throw new InvalidOperationException("Unknown work unit.")
			};
		}

		public static WorkEstimate From(decimal value, WorkUnit unit) =>
			new WorkEstimate(value, unit);

		protected override IEnumerable<object?> GetEqualityComponents()
		{
			yield return Value;
			yield return Unit;
		}
	}
}
