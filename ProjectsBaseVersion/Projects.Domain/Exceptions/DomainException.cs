﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Exceptions
{
	public class DomainException : Exception
	{
		public DomainException() { }

		public DomainException(string message) : base(message) { }

		public DomainException(string message, Exception innerException) : base(message, innerException) { }
	}
}
