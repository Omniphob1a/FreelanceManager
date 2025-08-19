using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Persistence.Models;

namespace Tasks.Persistence.Mappings
{
	public class TimeEntryMapper
	{
		public TimeEntryEntity ToEntity(TimeEntry entry, Guid taskId)
		{
			var entity = new TimeEntryEntity
			{
				Id = entry.Id,
				UserId = entry.UserId,
				TaskId = taskId,
				StartedAt = entry.Period.Start,
				EndedAt = entry.Period.End,
				Description = entry.Description,
				IsBillable = entry.IsBillable,
				CreatedAt = entry.CreatedAt,
				HourlyRateAmount = entry.HourlyRateSnapshot?.Amount,
				HourlyRateCurrency = entry.HourlyRateSnapshot?.Currency
			};

			return entity;
		}

		public TimeEntry ToDomain(TimeEntryEntity entity)
		{
			var range = TimeRange.Create(entity.StartedAt, entity.EndedAt);
			Money? rate = null;

			if (entity.HourlyRateAmount.HasValue && !string.IsNullOrWhiteSpace(entity.HourlyRateCurrency))
			{
				rate = Money.From(entity.HourlyRateAmount.Value, entity.HourlyRateCurrency);
			}

			return TimeEntry.Create(
				userId: entity.UserId,
				taskId: entity.TaskId,
				period: range,
				description: entity.Description,
				isBillable: entity.IsBillable,
				hourlyRateSnapshot: rate,
				createdAtUtc: entity.CreatedAt
			);
		}
	}
}
