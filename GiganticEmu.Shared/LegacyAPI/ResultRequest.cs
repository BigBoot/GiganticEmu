using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record ResultRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; init; } = default!;
    }
}
