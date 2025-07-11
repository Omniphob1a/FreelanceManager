using Projects.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.ValueObjects
{
	public sealed class Category : ValueObject
	{
		public string Value { get; }

		private static readonly HashSet<string> Allowed = new()
	{
		"design", "development", "marketing", "qa", "research"
	};

		private Category(string value) => Value = value;

		public static Category From(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ArgumentException("Category is required.");

			var val = input.Trim().ToLowerInvariant();
			if (!Allowed.Contains(val))
				throw new ArgumentException($"Invalid category: {val}");

			return new Category(val);
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}

		public override string ToString() => Value;
	}

}
