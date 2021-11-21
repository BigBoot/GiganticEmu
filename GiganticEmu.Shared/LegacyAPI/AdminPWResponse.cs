using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record AdminPWResponse
    {
        [JsonPropertyName("admin_pw")]
        public string AdminPW { get; init; } = default!;
    }
}