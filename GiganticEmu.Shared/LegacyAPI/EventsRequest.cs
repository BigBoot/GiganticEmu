using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record EventsRequest
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; init; } = default!;
    }
}