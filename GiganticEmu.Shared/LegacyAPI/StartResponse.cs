using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record StartResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; init; } = default!;

        [JsonPropertyName("open_url")]
        public string? OpenUrl { get; init; } = default!;
    }
}