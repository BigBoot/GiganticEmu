using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record KillResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; init; } = default!;
    }
}