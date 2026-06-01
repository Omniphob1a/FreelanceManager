using Users.Application.Users.Commands.RegisterUser;

namespace Users.UnitTests;

public class RegistrationValidationTests
{
	[Fact]
	public void RegistrationValidation_AcceptsValidData_AndRejectsInvalidData()
	{
		var validator = new RegisterUserCommandValidator();
		var valid = new RegisterUserCommand(
			Login: "VladimirShmolenko",
			Password: "Password123",
			Name: "Vladimir",
			Gender: 2,
			Birthday: new DateTime(2004, 9, 24, 0, 0, 0, DateTimeKind.Utc),
			Email: "vladimir@example.com",
			IsAdmin: false,
			CreatedBy: "self-registration");

		var invalid = valid with
		{
			Email = "not-an-email",
			Birthday = DateTime.UtcNow.AddDays(1),
			Name = "Vladimir Shmolenko"
		};

		Assert.True(validator.Validate(valid).IsValid);
		Assert.False(validator.Validate(invalid).IsValid);
	}
}
