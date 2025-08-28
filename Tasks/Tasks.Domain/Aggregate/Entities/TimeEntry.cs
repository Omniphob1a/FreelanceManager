using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.ValueObjects;

namespace Tasks.Domain.Aggregate.Entities
{
	public class TimeEntry 
	{
		public Guid Id { get; private set; }
		public Guid UserId { get; private set; }
		public Guid TaskId { get; private set; }
		public TimeRange Period { get; private set; }
		public string? Description { get; private set; }
		public bool IsBillable { get; private set; }
		public Money? HourlyRateSnapshot { get; private set; } 
		public DateTime CreatedAt { get; private set; }


		private TimeEntry(Guid id,
			Guid userId, 
			Guid taskId,
			TimeRange period,
			string? description,
			bool isBillable,
			Money? hourlyRateSnapshot,
			DateTime createdAt)
		{
			Id = id;
			UserId = userId;
			TaskId = taskId;
			Period = period;
			Description = description;
			IsBillable = isBillable;
			HourlyRateSnapshot = hourlyRateSnapshot;
			CreatedAt = createdAt;
		}

		public static TimeEntry Create(Guid userId, 
			Guid taskId,
			TimeRange period,
			string? description,
			bool isBillable,
			Money? hourlyRateSnapshot,
			DateTime createdAtUtc)
		{
			return new TimeEntry(
				Guid.NewGuid(), 
				userId, 
				taskId,
				period, 
				description,
				isBillable,
				hourlyRateSnapshot,
				createdAtUtc);
		}

		public TimeSpan Duration => Period.Duration;
	}
}
