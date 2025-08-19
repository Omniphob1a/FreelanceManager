using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Common;

namespace Tasks.Domain.Aggregate.ValueObjects
{
	public sealed class TimeRange : ValueObject
	{
		public DateTime Start { get; }
		public DateTime End { get; }

		private TimeRange(DateTime start, DateTime end)
		{
			if (end <= start)
				throw new ArgumentException("End must be after start");

			Start = start;
			End = end;
		}

		public static TimeRange Create(DateTime start, DateTime end) =>
			new TimeRange(start, end);

		public TimeSpan Duration => End - Start;

		protected override IEnumerable<object?> GetEqualityComponents()
		{
			yield return Start;
			yield return End;
		}
	}
}
