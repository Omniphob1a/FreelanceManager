using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Users.Application.Users.Commands.UpdateUser
{
	public record UpdateUserCommand(
		string NewName,
		int NewGender,
		DateTime NewBirthday,
		string NewEmail
	)

	{
		[JsonIgnore]
		public string ModifiedBy { get; init; } = string.Empty;
	}
}
