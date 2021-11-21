using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record EventsResponse
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; init; } = default!;

        [JsonPropertyName("events")]
        public object[] Events { get; init; } = default!;
    }
}