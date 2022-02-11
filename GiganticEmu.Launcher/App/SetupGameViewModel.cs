using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Force.Crc32;
using ICSharpCode.SharpZipLib.Zip;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class SetupGameViewModel : ReactiveObject
{
    
    [Reactive] 
    public float Progress { get; set; } = 0.0f;
    
    [Reactive] 
    public string? CurrentTask  { get; set; }
    
    [Reactive] 
    public string? ZipPath  { get; set; }
    
    [Reactive] 
    public string? InstallPath  { get; set; }
    
    [ObservableAsProperty] 
    public bool CanInstall  { get; }
    
    public ReactiveCommand<Unit, Unit> Install { get; }
    
    public Interaction<string, Unit> VerificationFailed { get; } = new();
    
    public Interaction<string, Unit> InstallationFailed { get; } = new();
    
    public Interaction<Unit, bool> CreateShortcut { get; } = new();
    
    public SetupGameViewModel()
    {
        Install = ReactiveCommand.CreateFromTask(DoInstall);

        this.WhenAnyValue(
            x => x.ZipPath,
            x => x.InstallPath,
            (zip, install) => (zip, install) switch
            {
                {zip: { }, install: { }} when File.Exists(zip) && install != "" => true,
                _ => false,
            }
        ).ToPropertyEx(this, x => x.CanInstall);
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
        
                RxApp.MainThreadScheduler.Schedule((float) read / length, (_, progress) =>
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
        }
        
        CurrentTask = "Extracting";
        
        await Task.Run(() =>
        {
            using var fsInput = File.OpenRead(ZipPath!);
            using var zf = new ZipFile(fsInput);
            
            var totalsize = zf.OfType<ZipEntry>().Sum(e => e.Size);
            var totalRead = 0L;
            
            foreach (var (entry, i) in zf.OfType<ZipEntry>().Select((entry, i) => (entry, i))) {
                if (!entry.IsFile) {
                    // Ignore directories
                    continue;
                }
                
                var entryFileName = entry.Name["Gigantic-Core_de\\".Length..];
        
                RxApp.MainThreadScheduler.Schedule($"Extracting {entryFileName}", (_, task) =>
                {
                    CurrentTask = task; 
                    return Disposable.Empty;
                });
        
                // Manipulate the output filename here as desired.
                var fullZipToPath = Path.Combine(InstallPath!, entryFileName);
                var directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName is { Length: >0}) {
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
        
                    RxApp.MainThreadScheduler.Schedule((float) totalRead / totalsize, (_, progress) =>
                    {
                        Progress = progress; 
                        return Disposable.Empty;
                    });
                }
                
                fsOutput.Flush();
            }
        });
        
        Progress = 1.0f;
        
        CurrentTask = "Installing dependencies";
        
        foreach (var file in new[]{"d3dx10_43.dll", "msvcp140.dll", "vcruntime140.dll"})
        {
            var sri = Application.GetResourceStream(new Uri($"pack://application:,,,/Resources/{file}"));
        
            if (sri is null)
            {
                await InstallationFailed.Handle($"Unable to install dependency {file}.");
                CurrentTask = null;
                return;
            }
            
            await using var input = sri.Stream!;
            await using var output = File.Create(Path.Join(InstallPath!, "Binaries", "Win64", file));
            await input.CopyToAsync(output);
        }
        
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
        Application.Current.Shutdown();
    }
}
