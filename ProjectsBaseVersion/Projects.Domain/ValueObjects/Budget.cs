using Projects.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.ValueObjects
{
	public class Budget : ValueObject
	{
		public decimal? Min { get; private set; }
		public decimal? Max { get; private set; }
		public string Currency { get; private set; }

		public Budget(decimal? min, decimal? max, string currency)
		{
			if (min.HasValue && max.HasValue && min > max)
				throw new ArgumentException("Minimum budget cannot exceed maximum.");

			Min = min;
			Max = max;
			Currency = currency ?? throw new ArgumentNullException(nameof(currency));
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Min ?? 0;
			yield return Max ?? 0;
			yield return Currency;
		}
	}
}
