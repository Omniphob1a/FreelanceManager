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
		public CurrencyCode CurrencyCode { get; private set; }

		public Budget(decimal? min, decimal? max, CurrencyCode currency)
		{
			if (min < 0 || max < min)
				throw new ArgumentException("Invalid budget range.");

			CurrencyCode = currency ?? throw new ArgumentNullException(nameof(currency));
			Min = min;
			Max = max;
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Min ?? 0;
			yield return Max ?? 0;
			yield return CurrencyCode;
		}
	}
}
