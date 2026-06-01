using System;
using System.Text.Json.Serialization;

namespace Users.Application.Messaging.Contracts
{
    // Сообщение: Projects -> Users
    // objectId   - идентификатор созданного объекта (Guid)
    // userId     - пользователь, который должен подтвердить (Guid)
    // replyTo    - куда отправить ответ (topic name)
    // timestampUtc - время отправки (UTC)
    public sealed record ConfirmRequestDto(
		[property: JsonPropertyName("objectId")] Guid ObjectId,
		[property: JsonPropertyName("userId")] Guid UserId,
		[property: JsonPropertyName("replyTo")] string ReplyTo,
		[property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc
	);
}
