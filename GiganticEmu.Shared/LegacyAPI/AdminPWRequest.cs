using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record AdminPWRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; init; } = default!;
    }
}