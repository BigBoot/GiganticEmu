using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record KillRequest
    {

        [JsonPropertyName("id")]
        public int Id { get; init; } = default!;
    }
}