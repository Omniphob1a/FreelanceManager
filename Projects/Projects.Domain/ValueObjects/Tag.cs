using Projects.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.ValueObjects
{
	public sealed class Tag : ValueObject
	{
		public string Value { get; }

		private Tag(string value) => Value = value;

		public static Tag From(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ArgumentException("Tag is required.");

			return new Tag(input.Trim().ToLowerInvariant());
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}

		public override string ToString() => Value;
	}

}
