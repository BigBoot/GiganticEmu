using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record ResultResponse
    {
        [JsonPropertyName("winner")]
        public string Winner { get; init; } = default!;
    }
}
