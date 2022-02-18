using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Force.Crc32;
using ICSharpCode.SharpZipLib.Zip;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class SetupGameViewModel : ReactiveObject
{
    [Reactive]
    public float Progress { get; set; } = 0.0f;

    [Reactive]
    public string? CurrentTask { get; set; }

    [Reactive]
    public string? ZipPath { get; set; }

    [Reactive]
    public string? InstallPath { get; set; }

    [ObservableAsProperty]
    public bool CanInstall { get; }

    public ReactiveCommand<Unit, Unit> Install { get; }

    public Interaction<string, Unit> VerificationFailed { get; } = new();

    public Interaction<string, Unit> InstallationFailed { get; } = new();

    public Interaction<Unit, bool> CreateShortcut { get; } = new();


    public Interaction<Unit, Unit> Quit { get; } = new();

    public SetupGameViewModel()
    {
        Install = ReactiveCommand.CreateFromTask(DoInstall);

        this.WhenAnyValue(
            x => x.ZipPath,
            x => x.InstallPath,
            (zip, install) => (zip, install) switch
            {
                { zip: { }, install: { } } when File.Exists(zip) && install != "" => true,
                _ => false,
            }
        ).ToPropertyEx(this, x => x.CanInstall);
    }

    private async Task ExtractZip(Stream input, string removePrefix = "", string addPrefix = "")
    {
        using var zf = new ZipFile(input);

        var totalsize = zf.OfType<ZipEntry>().Sum(e => e.Size);
        var totalRead = 0L;

        foreach (var (entry, i) in zf.OfType<ZipEntry>().Select((entry, i) => (entry, i)))
        {
            if (!entry.IsFile)
            {
                // Ignore directories
                continue;
            }

            var entryFileName = addPrefix + Regex.Replace(entry.Name, $"^{Regex.Escape(removePrefix)}", "");

            RxApp.MainThreadScheduler.Schedule($"Extracting {entryFileName}", (_, task) =>
            {
                CurrentTask = task;
                return Disposable.Empty;
            });

            // Manipulate the output filename here as desired.
            var fullZipToPath = Path.Combine(InstallPath!, entryFileName);
            var directoryName = Path.GetDirectoryName(fullZipToPath);
            if (directoryName is { Length: > 0 })
            {
                Directory.CreateDirectory(directoryName);
            }

            var buffer = new byte[1048576];

            using var zipStream = zf.GetInputStream(entry);
            using Stream fsOutput = File.Create(fullZipToPath);

            int bytesRead;
            while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fsOutput.Write(buffer, 0, bytesRead);
                totalRead += bytesRead;

                RxApp.MainThreadScheduler.Schedule((float)totalRead / totalsize, (_, progress) =>
               {
                   Progress = progress;
                   return Disposable.Empty;
               });
            }

            fsOutput.Flush();
        }
    }

    private async Task DoInstall()
    {
        CurrentTask = "Verifying";
        Progress = 0.0f;

        var hash = await Task.Run(() =>
        {
            uint hash = 0;
            var buffer = new byte[1048576];

            using var entryStream = File.OpenRead(ZipPath!);
            var length = entryStream.Length;
            var currentBlockSize = 0;
            var read = 0L;

            while ((currentBlockSize = entryStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                hash = Crc32CAlgorithm.Append(hash, buffer, 0, currentBlockSize);
                read += currentBlockSize;

                RxApp.MainThreadScheduler.Schedule((float)read / length, (_, progress) =>
               {
                   Progress = progress;
                   return Disposable.Empty;
               });
            }

            return hash;
        });

        Progress = 1.0f;

        if (hash != FileHashes.CORE_DE_ZIP_HASH)
        {
            await VerificationFailed.Handle("Verification of the zip file failed.");
            Progress = 0.0f;
            CurrentTask = null;
            return;
        }

        CurrentTask = "Extracting";

        await Task.Run(async () =>
        {
            using var fsInput = File.OpenRead(ZipPath!);
            await ExtractZip(fsInput, removePrefix: "Gigantic-Core_de/");
        });

        Progress = 1.0f;

        CurrentTask = "Installing dependencies";

        await Task.Run(async () =>
        {
            var assets = Locator.Current.RequireService<IAssetLoader>();
            var input = assets.TryOpen($"/Resources/redist.zip");

            if (input is null)
            {
                await InstallationFailed.Handle($"Unable to install dependencies.");
                CurrentTask = null;
                return;
            }

            await ExtractZip(input, addPrefix: "Binaries/Win64/");
        });

        CurrentTask = "Installing Mistforge Launcher";

        await Task.Run(async () =>
        {
            var launcherLocation = Environment.ProcessPath!;
            await using var input = File.OpenRead(launcherLocation);
            await using var output = File.Create(Path.Join(InstallPath!, "MistforgeLauncher.exe"));
            await input.CopyToAsync(output);
        });

        if (await CreateShortcut.Handle(Unit.Default))
        {
            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var filename = Path.Combine(desktopFolder, "Mistforge Launcher.lnk");
            var target = Path.Join(InstallPath!, "MistforgeLauncher.exe");

            Lnk.Create(filename, target);
        }

        CurrentTask = null;

        Process.Start(new ProcessStartInfo
        {
            FileName = Path.Join(InstallPath!, "MistforgeLauncher.exe"),
            WorkingDirectory = InstallPath,
        });

        await Quit.Handle(Unit.Default);
    }
}