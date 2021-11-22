using GiganticEmu.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;

namespace GiganticEmu.Launcher
{
    public class UserViewModel : ReactiveObject
    {
        [Reactive]
        public bool IsLoading { get; set; } = false;

        [Reactive]
        public UserData? User { get; set; } = null;

        public ReactiveCommand<Unit, Unit> Logout { get; }

        public ReactiveCommand<Unit, Unit> StartGame { get; }

        public UserViewModel()
        {
            Logout = ReactiveCommand.CreateFromTask(DoLogout);
            StartGame = ReactiveCommand.CreateFromTask(DoStartGame);
        }

        private async Task DoLogout()
        {
            IsLoading = true;
            await Locator.Current.GetService<CredentialStorage>()!.ClearToken();
            User = null;
            IsLoading = false;
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
    }
}
