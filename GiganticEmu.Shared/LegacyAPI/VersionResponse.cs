using System.Text.Json.Serialization;

namespace GiganticEmu.Shared.LegacyApi
{
    public record VersionResponse
    {
        [JsonPropertyName("app_version")]
        public string AppVersion { get; init; } = default!;

        [JsonPropertyName("app_version_major")]
        public int AppVersionMajor { get; init; } = default!;

        [JsonPropertyName("app_version_minor")]
        public int AppVersionMinor { get; init; } = default!;

        [JsonPropertyName("app_version_patch")]
        public int AppVersionPatch { get; init; } = default!;

        [JsonPropertyName("api_version")]
        public int ApiVersion { get; init; } = default!;
    }
}