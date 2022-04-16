using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Splat;

namespace GiganticEmu.Launcher;

public class AutoUpdater
{
    public record UpdateInfo(SemVer Version, GitHub.Changelog Changelog);

    private async Task<GitHub.Changelog?> GetChangelog(SemVer version)
    {
        var github = Locator.Current.RequireService<GitHub>();
        return await github.GetChangelog(version);
    }

    public IObservable<double> DownloadUpdate(SemVer version)
    {
        var config = Locator.Current.RequireService<LauncherConfiguration>();
        var github = Locator.Current.RequireService<GitHub>();
        var filename = $"MistforgeLauncher{PlatformUtils.ExecutableExtension}";
        var targetDir = config.Game;
        var targetFileName = $"_upd.MistforgeLauncher{PlatformUtils.ExecutableExtension}";
        var observable = new BehaviorSubject<double>(0.0);

        _ = Task.Run(async () =>
        {
            await github.DownloadFile(version, filename, targetDir, targetFileName, progress => observable.OnNext(progress));
            observable.OnCompleted();

            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Join(targetDir, targetFileName),
                    WorkingDirectory = targetDir,
                },
            }.Start();
            App.Current?.Shutdown(0);
        });

        return observable;
    }

    public async Task<bool> IsUpdatePending()
    {
        var path = Environment.ProcessPath!;
        var updateFile = Path.Join(Path.GetDirectoryName(path), $"_upd.{Path.GetFileName(path)}");

        if (File.Exists(updateFile))
        {
            File.Delete(updateFile);
        }

        return Path.GetFileName(Environment.ProcessPath!).StartsWith("_upd.");
    }

    public async Task ApplyPendingUpdate()
    {
        var source = Environment.ProcessPath!;
        var target = Path.Join(Path.GetDirectoryName(source), Path.GetFileName(source).Substring("_upd.".Length));

        using (var input = File.OpenRead(source))
        using (var output = File.Create(target))
        {
            await input.CopyToAsync(output);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = target,
            WorkingDirectory = new FileInfo(target).DirectoryName,
        });

        App.Current?.Shutdown();
    }

    public async Task<UpdateInfo?> CheckForUpdate()
    {
        var github = Locator.Current.RequireService<GitHub>();

        if (await github.GetLatestRelease() is GitHub.ReleaseInfo info)
        {
            if (SemVer.TryParse(Regex.Replace(info.TagName, "^v", "")) is SemVer latest && latest > SemVer.ApplicationVersion)
            {
                if (await GetChangelog(latest) is { } changelog)
                {
                    return new UpdateInfo(latest, changelog);
                }
            }
        }

        return null;
    }

    public async Task SkipUpdate(SemVer version)
    {
        var settings = Locator.Current.RequireService<Settings>();
        settings.AutoUpdaterSkippedVersion.OnNext(version);
        await settings.Save();
    }

    public async Task RemindLater(TimeSpan duration)
    {
        var settings = Locator.Current.RequireService<Settings>();
        settings.AutoUpdaterRemindLater.OnNext(DateTime.Now.Add(duration));
        await settings.Save();
    }
}