using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record StartRequest
    {
        [JsonPropertyName("map")]
        public string Map { get; init; } = default!;

        [JsonPropertyName("max_players")]
        public int? MaxPlayers { get; init; } = default!;

        [JsonPropertyName("creature0")]
        public string? Creature0 { get; init; } = default!;

        [JsonPropertyName("creature1")]
        public string? Creature1 { get; init; } = default!;

        [JsonPropertyName("creature2")]
        public string? Creature2 { get; init; } = default!;

        [JsonPropertyName("report_url")]
        public string? ReportUrl { get; init; } = default!;

        public (string, string, string)? Creatures
        {
            get => Creature0 != null && Creature1 != null && Creature2 != null ? (Creature0, Creature1, Creature2) : null;
        }
    }
}