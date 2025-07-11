using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.ChangeUserPassword
{
	public record ChangeUserPasswordCommand(
		string NewPassword
	)

	{
		[JsonIgnore]
		public string ModifiedBy { get; init; } = string.Empty;
	}
}
