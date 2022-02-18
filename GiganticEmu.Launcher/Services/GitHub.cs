using System.Threading.Tasks;
using GiganticEmu.Shared;
using System.IO;
using Polly;
using System.Text.Json.Serialization;
using Flurl.Http;
using KeepAChangelogParser;
using KeepAChangelogParser.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GiganticEmu.Launcher;

public class GitHub
{
    public record ReleaseInfo
    {
        [JsonPropertyName("id")]
        public long Id { get; set; } = default!;

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = default!;

        [JsonPropertyName("update_url")]
        public string UpdateUrl { get; set; } = default!;

        [JsonPropertyName("update_authenticity_token")]
        public string UpdateAuthenticityToken { get; set; } = default!;

        [JsonPropertyName("delete_url")]
        public string DeleteUrl { get; set; } = default!;

        [JsonPropertyName("delete_authenticity_token")]
        public string DeleteAuthenticityToken { get; set; } = default!;

        [JsonPropertyName("edit_url")]
        public string EditUrl { get; set; } = default!;
    }

    public record Changelog(IEnumerable<Changelog.Version> versions)
    {
        public record Version(SemVer VersionNumber, DateTime Date, IEnumerable<Section> Sections);
        public record Section(string Title, IEnumerable<string> Entries);
    }

    public async Task<ReleaseInfo> GetLatestRelease()
    {
        return await "https://github.com/BigBoot/GiganticEmu/releases/latest"
             .WithHeader("Accept", "application/json")
             .ConfigureRequest(request => request.Redirects.ForwardHeaders = true)
             .WithPolly(policy => policy.RetryAsync(3))
             .GetJsonAsync<ReleaseInfo>();
    }

    public async Task<Changelog?> GetChangelog(SemVer version)
    {
        var markdown = await $"https://raw.githubusercontent.com/BigBoot/GiganticEmu/v{version}/CHANGELOG.md"
            .WithPolly(policy => policy.RetryAsync(3))
            .GetStringAsync();

        var parser = new ChangelogParser();
        if (parser.Parse(markdown).Value is { } changelog)
        {
            var versions = changelog.SectionCollection.Select(section =>
            {
                var versionNumber = SemVer.TryParse(section.MarkdownVersion) ?? new SemVer(0, 0, 0);
                DateTime.TryParseExact(section.MarkdownDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);

                var sections = section.SubSectionCollection.Select(subsection =>
                {
                    var title = subsection.Type.ToString();
                    var items = subsection.ItemCollection.Select(item => item.MarkdownText);
                    return new Changelog.Section(title, items);
                });

                return new Changelog.Version(versionNumber, date, sections);
            });

            return new Changelog(versions);
        }

        return null;
    }

    public async Task DownloadFile(SemVer version, string filename, string targetDir, string? targetFileName = null, Action<double>? progressCallback = null)
    {
        await $"https://github.com/BigBoot/GiganticEmu/releases/download/v{version.ToString()}/{filename}"
            .WithPolly(policy => policy.RetryAsync(3))
            .DownloadFileAsync(targetDir, targetFileName ?? filename, progressCallback: progressCallback);
    }
}
