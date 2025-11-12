using Tasks.Domain.Aggregate.Events;
using Users.Domain.Entities;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserProfileUpdatedDomainEvent(Guid UserId, string NewName, int NewGender,	DateTime NewBirthday, string NewEmail) : DomainEvent(UserId, nameof(User))
	{
		public override string EventType => "users.profile_changed";
		public override string? KafkaTopic => "users";
		public override string? KafkaKey => UserId.ToString();
	}
}
