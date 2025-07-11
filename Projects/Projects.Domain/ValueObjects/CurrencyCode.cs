using Projects.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.ValueObjects
{
	public sealed class CurrencyCode : ValueObject
	{
		public string Code { get; }

		private static readonly HashSet<string> Allowed = new()
	{
		"USD", "EUR", "RUB"
	};

		private CurrencyCode(string code)
		{
			Code = code.ToUpperInvariant();
		}

		public static CurrencyCode From(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ArgumentException("Currency code is required.");

			var normalized = input.Trim().ToUpperInvariant();
			if (!Allowed.Contains(normalized))
				throw new ArgumentException($"Unsupported currency: {normalized}");

			return new CurrencyCode(normalized);
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Code;
		}

		public override string ToString() => Code;
	}
}
