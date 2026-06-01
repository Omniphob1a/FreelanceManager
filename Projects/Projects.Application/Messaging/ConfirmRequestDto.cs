using System;
using System.Text.Json.Serialization;

namespace Projects.Application.Messaging.Contracts
{
    public sealed record ConfirmRequestDto(
		[property: JsonPropertyName("objectId")] Guid ObjectId,
		[property: JsonPropertyName("userId")] Guid UserId,
		[property: JsonPropertyName("replyTo")] string ReplyTo,
		[property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc
	);
}
