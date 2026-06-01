using System.Text.Json.Serialization;

namespace Projects.Application.Messaging.Contracts
{
	public sealed record ConfirmResponseDto(
		[property: JsonPropertyName("objectId")] Guid ObjectId,
		[property: JsonPropertyName("confirmedAt")] DateTime ConfirmedAt,
		[property: JsonPropertyName("registeredObjects")] int? RegisteredObjects
	);
}
