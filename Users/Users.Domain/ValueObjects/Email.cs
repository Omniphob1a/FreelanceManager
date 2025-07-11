namespace Users.Domain.ValueObjects
{
	public sealed class Email
	{
		public string Value { get; }
		public Email(string value)
		{
			if (string.IsNullOrEmpty(value) || !value.Contains("@"))
			{
				throw new ArgumentException("Wrong Email");
			}

			Value = value;
		}
		public override string ToString() => Value;

		public override bool Equals(Object obj)
		{
			return obj is Email other &&
				   StringComparer.Ordinal.Equals(Value, other.Value);
		}
		public override int GetHashCode() => Value.GetHashCode();
		public static bool TryParse(string? value, out Email? email)
		{
			email = null;

			if (string.IsNullOrWhiteSpace(value))
				return false;

			try
			{
				var addr = new System.Net.Mail.MailAddress(value);
				if (addr.Address != value)
					return false;

				email = new Email(value);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
