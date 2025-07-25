﻿namespace Projects.Domain.Common
{
	public abstract class ValueObject
	{
		protected abstract IEnumerable<object> GetEqualityComponents();

		public override bool Equals(object? obj)
		{
			if (obj is null || obj.GetType() != GetType())
				return false;

			var other = (ValueObject)obj;

			return GetEqualityComponents()
				.SequenceEqual(other.GetEqualityComponents());
		}

		public override int GetHashCode()
		{
			return GetEqualityComponents()
				.Aggregate(1, (current, obj) =>
				{
					unchecked
					{
						return current * 23 + (obj?.GetHashCode() ?? 0);
					}
				});
		}

		public static bool operator ==(ValueObject? a, ValueObject? b)
		{
			return a?.Equals(b) ?? b is null;
		}

		public static bool operator !=(ValueObject? a, ValueObject? b)
		{
			return !(a == b);
		}
	}
}