using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using GiganticEmu.Agent;
using GiganticEmu.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Splat;

namespace GiganticEmu.Launcher;

public static class GameLauncher
{
    public static bool GameFound =>
        File.Exists(Path.Join(Locator.Current.GetService<LauncherConfiguration>()!.Game, "Binaries", "Win64",
            "RxGame-Win64-Test.exe"));

    public static async Task RunGame(string? username = null, string? nickname = null, string? authToken = null)
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

        if (arcsdkMetadata is not { ProductName: "ArcSDK" } || arcsdkMetadata.ProductVersion == null)
        {
            var result = MessageBox.Show("Unknown ArcSDK.dll version found.\nThis probably means this is your first time running MistforgeLauncher.\nMistforgeLauncher needs to replace the ArcSDK.dll with it's own version to continue.\n\nDo you want to continue?", "Unknown ArcSDK.dll version!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            await github.DownloadFile(SemVer.ApplicationVersion, "ArcSDK.dll", path);
        }
        else
        {
            var arcSdkVersion = SemVer.Parse(arcsdkMetadata.ProductVersion);

            if (SemVer.ApplicationVersion != arcSdkVersion)
            {
                var result = MessageBox.Show("Mismatched ArcSDK.dll version detected. The version of MistforgeLauncher and ArcSDK.dll should match or problems can occur.\n\nShould MistforgeLauncher replace your ArcSDK.dll with the correct version?", "Mismatched ArcSDK.dll version!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await github.DownloadFile(SemVer.ApplicationVersion, "ArcSDK.dll", path);
                }
            }
        }

        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = Path.Join(path, "RxGame-Win64-Test.exe")
        };

        process.StartInfo.ArgumentList.Add($"?name={nickname ?? "Offline"}");

        process.StartInfo.ArgumentList.Add($"-ini:RxEngine:MotigaAuthIntegration.AuthUrlPrefix={config.Host}/,ArcIntegration.AuthUrlPrefix={config.Host}/");
        process.StartInfo.ArgumentList.Add($"-log=GiganticEmu.Launcher.{username ?? "offline"}.log");

        process.StartInfo.ArgumentList.Add($"-emu:nickname={nickname ?? "Offline"}");
        process.StartInfo.ArgumentList.Add($"-emu:username={username ?? "Offline"}");
        process.StartInfo.ArgumentList.Add($"-emu:auth_token={authToken ?? ""}");
        process.StartInfo.ArgumentList.Add(@$"-emu:language={Settings.GameLanguage switch
        {
            Settings.Language.English => "INT",
            Settings.Language.German => "DEU",
            Settings.Language.French => "FRA",
        }}");
        process.StartInfo.ArgumentList.Add($"-emu:launch_code={(giganticMetadata.ProductBuildPart == 16601 ? 0 : 0xE0000019)}");

        process.Start();
        await process.WaitForExitAsync();
    }

    public static void StartGame(string? username = null, string? nickname = null, string? authToken = null)
    {
        _ = Task.Run(async () => await GameLauncher.RunGame(username, nickname, authToken));
    }

    public static async Task RunServer(int port)
    {
        var logger = Locator.Current.GetService<ILogger<MainContainerViewModel>>()!;
        var config = Locator.Current.GetService<LauncherConfiguration>()!;
        var path = Path.GetFullPath(Path.Join(config.Game));

        if (!File.Exists(Path.Join(path, "Binaries", "Win64", "RxGame-Win64-Test.exe")))
        {
            MessageBox.Show($"{Path.Join(path, "RxGame-Win64-Test.exe")} not found!", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        await Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new List<KeyValuePair<string, string>> {
                            new(nameof(AgentConfiguration.WebPort), port.ToString()),
                            new(nameof(AgentConfiguration.GiganticPath), path),
                    })
                    .Build();
                var agentConfiguration = new AgentConfiguration();
                configuration.Bind(agentConfiguration, o => o.BindNonPublicProperties = true);

                webBuilder.UseConfiguration(configuration);
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls($"http://127.0.0.1:{agentConfiguration.WebPort}/");
            }).Build().RunAsync().LogExceptions(logger);
    }

    public static void StartServer(int port)
    {
        _ = Task.Run(async () => await GameLauncher.RunServer(port));
    }
}