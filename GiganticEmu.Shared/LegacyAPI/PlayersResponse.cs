using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record PlayersResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = default!;

        [JsonPropertyName("hero")]
        public string? hero { get; init; } = default!;
    }
}