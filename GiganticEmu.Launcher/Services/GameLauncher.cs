using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Splat;

namespace GiganticEmu.Launcher;

public class GameLauncher
{
    public static bool GameFound =>
        File.Exists(Path.Join(Locator.Current.RequireService<LauncherConfiguration>().Game, "Binaries", "Win64",
            "RxGame-Win64-Test.exe"));

    public async Task RunGame(string? username = null, string? nickname = null, string? authToken = null)
    {
        var config = Locator.Current.RequireService<LauncherConfiguration>();
        var settings = Locator.Current.RequireService<Settings>();
        var github = Locator.Current.RequireService<GitHub>();
        var path = Path.GetFullPath(Path.Join(config.Game, "Binaries", "Win64"));

        if (!File.Exists(Path.Join(path, "RxGame-Win64-Test.exe")))
        {
            // MessageBox.Show($"{Path.Join(path, "RxGame-Win64-Test.exe")} not found!", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var giganticMetadata = FileVersionInfo.GetVersionInfo(Path.Join(path, "RxGame-Win64-Test.exe"));

        var arcsdkMetadata = File.Exists(Path.Join(path, "ArcSDK.dll")) ? FileVersionInfo.GetVersionInfo(Path.Join(path, "ArcSDK.dll")) : null;

        if (arcsdkMetadata is not { ProductName: "ArcSDK" } || arcsdkMetadata.ProductVersion == null)
        {
            // var result = MessageBox.Show("Unknown ArcSDK.dll version found.\nThis probably means this is your first time running MistforgeLauncher.\nMistforgeLauncher needs to replace the ArcSDK.dll with it's own version to continue.\n\nDo you want to continue?", "Unknown ArcSDK.dll version!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            // if (result != MessageBoxResult.Yes) return;

            await github.DownloadFile(SemVer.ApplicationVersion, "ArcSDK.dll", path);
        }
        else
        {
            var arcSdkVersion = SemVer.Parse(arcsdkMetadata.ProductVersion);

            if (SemVer.ApplicationVersion != arcSdkVersion)
            {
                // var result = MessageBox.Show("Mismatched ArcSDK.dll version detected. The version of MistforgeLauncher and ArcSDK.dll should match or problems can occur.\n\nShould MistforgeLauncher replace your ArcSDK.dll with the correct version?", "Mismatched ArcSDK.dll version!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                // if (result == MessageBoxResult.Yes)
                // {
                //     await github.DownloadFile(SemVer.ApplicationVersion, "ArcSDK.dll", path);
                // }
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
        process.StartInfo.ArgumentList.Add(@$"-emu:language={settings.GameLanguage.Value switch
        {
            Settings.Language.English => "INT",
            Settings.Language.German => "DEU",
            Settings.Language.French => "FRA",
        }}");
        process.StartInfo.ArgumentList.Add($"-emu:launch_code={(giganticMetadata.ProductBuildPart == 16601 ? 0 : 0xE0000019)}");

        process.Start();
        await process.WaitForExitAsync();
    }

    public void StartGame(string? username = null, string? nickname = null, string? authToken = null)
    {
        _ = Task.Run(async () => await RunGame(username, nickname, authToken));
    }
}