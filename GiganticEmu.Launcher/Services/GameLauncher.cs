using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Splat;
using PeNet;
using System.Linq;
using System;
using System.Collections.Generic;

namespace GiganticEmu.Launcher;

public class GameLauncher
{
    public record InteractionHandler
    {
        public Func<DialogContent, Task> OnError { get; init; } = async (_) => { };
        public Func<DialogContent, Task<bool>> OnConfirm { get; init; } = async (_) => false;
    }

    public static bool GameFound =>
        File.Exists(Path.Join(Locator.Current.RequireService<LauncherConfiguration>().Game, "Binaries", "Win64",
            "RxGame-Win64-Test.exe"));

    public async Task RunGame(InteractionHandler interactions, string? username = null, string? nickname = null, string? authToken = null)
    {
        var config = Locator.Current.RequireService<LauncherConfiguration>();
        var settings = Locator.Current.RequireService<Settings>();
        var github = Locator.Current.RequireService<GitHub>();
        var path = Path.GetFullPath(Path.Join(config.Game, "Binaries", "Win64"));

        try
        {

            if (!File.Exists(Path.Join(path, "RxGame-Win64-Test.exe")))
            {
                await interactions.OnError(new DialogContent
                {
                    Title = "File not found!",
                    Text = $"{Path.Join(path, "RxGame-Win64-Test.exe")} not found!",
                });
                return;
            }

            var giganticMetadata = new PeFile(Path.Join(path, "RxGame-Win64-Test.exe"))?.Resources?.VsVersionInfo?.StringFileInfo?.StringTable?.FirstOrDefault();

            var arcsdkMetadata = File.Exists(Path.Join(path, "ArcSDK.dll")) ? new PeFile(Path.Join(path, "ArcSDK.dll"))?.Resources?.VsVersionInfo?.StringFileInfo?.StringTable?.FirstOrDefault() : null;

            if (arcsdkMetadata is not { ProductName: "ArcSDK" } || arcsdkMetadata.ProductVersion == null)
            {
                var result = await interactions.OnConfirm(new DialogContent
                {
                    Title = "Unknown ArcSDK.dll version!",
                    Text = "Unknown ArcSDK.dll version found.\nThis probably means this is your first time running MistforgeLauncher.\nMistforgeLauncher needs to replace the ArcSDK.dll with it's own version to continue.\n\nDo you want to continue?"
                });

                if (!result)
                {
                    return;
                }

                await github.DownloadFile(SemVer.ApplicationVersion, "ArcSDK.dll", path);
            }
            else
            {
                var arcSdkVersion = SemVer.Parse(arcsdkMetadata.ProductVersion);

                if (SemVer.ApplicationVersion != arcSdkVersion)
                {
                    var result = await interactions.OnConfirm(new DialogContent
                    {
                        Title = "Mismatched ArcSDK.dll version!",
                        Text = "Mismatched ArcSDK.dll version detected. The version of MistforgeLauncher and ArcSDK.dll should match or problems can occur.\n\nShould MistforgeLauncher replace your ArcSDK.dll with the correct version?"
                    });
                    if (result)
                    {
                        await github.DownloadFile(SemVer.ApplicationVersion, "ArcSDK.dll", path);
                    }
                }
            }

            var commands = new List<string>();

            var process = new Process();

            if (PlatformUtils.IsLinux)
            {
                if (settings.LinuxEnableGameMode.Value)
                {
                    commands.Add("gamemoderun");
                }

                if (settings.LinuxEnableMangoHud.Value)
                {
                    commands.Add("mangohud");
                }

                commands.AddRange(settings.LinuxCompatiblityTool.Value switch
                {
                    Settings.CompatiblityTool.Proton => new[] { "proton", "run" },
                    Settings.CompatiblityTool.Wine => new[] { "wine" },

                });
            }

            commands.Add(Path.Join(path, "RxGame-Win64-Test.exe"));
            commands.Add($"?name={nickname ?? "Offline"}");

            commands.Add($"-ini:RxEngine:MotigaAuthIntegration.AuthUrlPrefix={config.Host}/,ArcIntegration.AuthUrlPrefix={config.Host}/");
            commands.Add($"-log=GiganticEmu.Launcher.{username ?? "offline"}.log");

            commands.Add($"-emu:nickname={nickname ?? "Offline"}");
            commands.Add($"-emu:username={username ?? "Offline"}");
            commands.Add($"-emu:auth_token={authToken ?? ""}");
            commands.Add(@$"-emu:language={settings.GameLanguage.Value switch
            {
                Settings.Language.English => "INT",
                Settings.Language.German => "DEU",
                Settings.Language.French => "FRA",
            }}");
            commands.Add($"-emu:launch_code={(giganticMetadata?.ProductVersion == "1, 0, 16601, 0" ? 0 : 0xE0000019)}");


            process.StartInfo = new ProcessStartInfo
            {
                FileName = commands.First()
            };

            foreach (var arg in commands.Skip(1))
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            interactions?.OnError(new DialogContent
            {
                Title = "Error during startup!",
                Text = ex.Message,
            });
        }
    }

    public void StartGame(InteractionHandler interactions, string? username = null, string? nickname = null, string? authToken = null)
    {
        _ = Task.Run(async () => await RunGame(interactions, username, nickname, authToken));
    }
}