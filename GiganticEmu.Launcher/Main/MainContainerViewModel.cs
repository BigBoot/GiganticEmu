using GiganticEmu.Agent;
using GiganticEmu.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;

namespace GiganticEmu.Launcher;

public class MainContainerViewModel : ReactiveObject
{
    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public UserData? User { get; set; } = null;

    public ReactiveCommand<Unit, Unit> StartGame { get; }

    public ReactiveCommand<Unit, Unit> StartServer { get; }

    private int? _serverPort;

    public MainContainerViewModel()
    {
        StartGame = ReactiveCommand.CreateFromTask(DoStartGame);
        StartServer = ReactiveCommand.CreateFromTask(DoStartServer);

        _serverPort = null;
    }

    private async Task DoStartGame()
    {
        var config = Locator.Current.GetService<LauncherConfiguration>()!;
        var github = Locator.Current.GetService<GitHub>()!;
        var path = Path.GetFullPath(Path.Join(config.Game, "Binaries", "Win64"));

        if (!File.Exists(Path.Join(path, "RxGame-Win64-Test.exe")))
        {
            MessageBox.Show($"{Path.Join(path, "RxGame-Win64-Test.exe")} not found!", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var giganticMetadata = FileVersionInfo.GetVersionInfo(Path.Join(path, "RxGame-Win64-Test.exe"));

        var arcsdkMetadata = File.Exists(Path.Join(path, "ArcSDK.dll")) ? FileVersionInfo.GetVersionInfo(Path.Join(path, "ArcSDK.dll")) : null;

        if (arcsdkMetadata == null || arcsdkMetadata.InternalName != "ArcSDK" || arcsdkMetadata.ProductVersion == null)
        {
            var result = MessageBox.Show("Unknown ArcSDK.dll version found.\nThis probably means this is your first time running MistforgeLauncher.\nMistforgeLauncher needs to replace the ArcSDK.dll with it's own version to continue.\n\nDo you want to continue?", "Unknown ArcSDK.dll version!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            await github.DownloadFile(Version.ApplicationVersion, "ArcSDK.dll", path);
        }
        else
        {
            var arcSdkVersion = Version.Parse(arcsdkMetadata.ProductVersion);

            if (Version.ApplicationVersion != arcSdkVersion)
            {
                var result = MessageBox.Show("Mismatched ArcSDK.dll version detected. The version of MistforgeLauncher and ArcSDK.dll should match or problems can occur.\n\nShould MistforgeLauncher replace your ArcSDK.dll with the correct version?", "Mismatched ArcSDK.dll version!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await github.DownloadFile(Version.ApplicationVersion, "ArcSDK.dll", path);
                }
            }
        }

        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = Path.Join(path, "RxGame-Win64-Test.exe")
        };

        process.StartInfo.ArgumentList.Add($"-ini:RxEngine:MotigaAuthIntegration.AuthUrlPrefix={config.Host}/,ArcIntegration.AuthUrlPrefix={config.Host}/");
        process.StartInfo.ArgumentList.Add($"-log=GiganticEmu.Launcher.{User!.UserName}.log");

        process.StartInfo.ArgumentList.Add($"-emu:nickname={User?.UserName}");
        process.StartInfo.ArgumentList.Add($"-emu:username={User?.UserName}");
        process.StartInfo.ArgumentList.Add($"-emu:auth_token={User?.AuthToken}");
        process.StartInfo.ArgumentList.Add($"-emu:launch_code={(giganticMetadata.ProductBuildPart == 16601 ? 0 : 0xE0000019)}");

        process.Start();
    }

    private async Task DoStartServer()
    {
        var logger = Locator.Current.GetService<ILogger<MainContainerViewModel>>()!;
        var config = Locator.Current.GetService<LauncherConfiguration>()!;
        var path = Path.GetFullPath(Path.Join(config.Game));

        if (!File.Exists(Path.Join(path, "Binaries", "Win64", "RxGame-Win64-Test.exe")))
        {
            MessageBox.Show($"{Path.Join(path, "RxGame-Win64-Test.exe")} not found!", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_serverPort == null)
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            _serverPort = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();


            var _ = Task.Run(async () => Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(new List<KeyValuePair<string, string>> {
                                new KeyValuePair<string, string>(nameof(AgentConfiguration.WebPort), _serverPort?.ToString() ?? ""),
                                new KeyValuePair<string, string>(nameof(AgentConfiguration.GiganticPath), path),
                        })
                        .Build();
                    var agentConfiguration = new AgentConfiguration();
                    configuration.Bind(agentConfiguration, o => o.BindNonPublicProperties = true);

                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://127.0.0.1:{agentConfiguration.WebPort}/");
                }).Build().RunAsync()).LogExceptions(logger);
        }

        System.Diagnostics.Process.Start(new ProcessStartInfo
        {
            FileName = $"http://127.0.0.1:{_serverPort}/",
            UseShellExecute = true,
        });
    }
}
