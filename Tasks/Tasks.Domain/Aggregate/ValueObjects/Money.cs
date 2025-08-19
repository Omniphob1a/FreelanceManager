using Tasks.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Domain.Aggregate.ValueObjects
{
	public sealed class Money : ValueObject
	{
		public decimal Amount { get; }
		public string Currency { get; }

		private Money(decimal amount, string currency)
		{
			if (string.IsNullOrWhiteSpace(currency))
				throw new ArgumentException("Currency is required", nameof(currency));

			if (amount < 0)
				throw new ArgumentException("Amount cannot be negative", nameof(amount));

			Amount = amount;
			Currency = currency.ToUpperInvariant();
		}

		public static Money From(decimal amount, string currency) =>
			new Money(amount, currency);

		protected override IEnumerable<object?> GetEqualityComponents()
		{
			yield return Amount;
			yield return Currency;
		}
	}
}
