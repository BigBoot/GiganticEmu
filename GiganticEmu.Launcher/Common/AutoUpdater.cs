using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GiganticEmu.Launcher;

public static class AutoUpdater
{
    private record GithubReleaseInfo
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

    private class GithubAuth : AutoUpdaterDotNET.IAuthentication
    {
        public void Apply(ref AutoUpdaterDotNET.MyWebClient webClient)
        {
            webClient.Headers["Accept"] = "application/json";
        }
    }

    private class PersistanceProvider : AutoUpdaterDotNET.IPersistenceProvider
    {
        public DateTime? GetRemindLater() => Settings.AutoUpdaterRemindLater;

        public Version? GetSkippedVersion() => Settings.AutoUpdaterSkippedVersion;

        public void SetRemindLater(DateTime? remindLaterAt)
        {
            Settings.AutoUpdaterRemindLater = remindLaterAt;
            Settings.Save();
        }

        public void SetSkippedVersion(Version version)
        {
            Settings.AutoUpdaterSkippedVersion = version;
            Settings.Save();
        }
    }

    public static void Check()
    {
        AutoUpdaterDotNET.AutoUpdater.AppTitle = "Mistforge Launcher";

        AutoUpdaterDotNET.AutoUpdater.InstalledVersion = new System.Version(
            int.Parse(GitVersionInformation.Major),
            int.Parse(GitVersionInformation.Minor),
            int.Parse(GitVersionInformation.Patch)
        );

        try
        {
            System.Security.AccessControl.DirectorySecurity ds = new FileInfo(Environment.ProcessPath!)
                .Directory!
                .GetAccessControl();

            AutoUpdaterDotNET.AutoUpdater.RunUpdateAsAdmin = false;
        }
        catch (UnauthorizedAccessException)
        {
            AutoUpdaterDotNET.AutoUpdater.RunUpdateAsAdmin = true;
        }

        AutoUpdaterDotNET.AutoUpdater.InstallationPath = new FileInfo(Environment.ProcessPath!).DirectoryName;

        AutoUpdaterDotNET.AutoUpdater.PersistenceProvider = new PersistanceProvider();

        AutoUpdaterDotNET.AutoUpdater.ParseUpdateInfoEvent += (args) =>
        {
            var json = JsonSerializer.Deserialize<GithubReleaseInfo>(args.RemoteData)!;

            args.UpdateInfo = new AutoUpdaterDotNET.UpdateInfoEventArgs
            {
                CurrentVersion = json.TagName.Substring(1),
                ChangelogURL = "https://github.com/BigBoot/GiganticEmu/releases",
                DownloadURL = $"https://github.com/BigBoot/GiganticEmu/releases/download/{json.TagName}/MistforgeLauncher.exe",
                InstallerArgs = $"--update-target=\"{Environment.ProcessPath!}\"",
            };
        };

        AutoUpdaterDotNET.AutoUpdater.BasicAuthXML = new GithubAuth();
        AutoUpdaterDotNET.AutoUpdater.Start("https://github.com/BigBoot/GiganticEmu/releases/latest");
    }
}