using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text.Json;
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
            var path = Path.GetFullPath(Path.Join(config.Game, "Binaries", "Win64"));

            if (!File.Exists(Path.Join(path, "RxGame-Win64-Test.exe")))
            {
                MessageBox.Show($"{Path.Join(path, "RxGame-Win64-Test.exe")} not found!", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userinfo = JsonSerializer.Serialize(new
            {
                nickname = User?.UserName,
                username = User?.UserName,
                auth_token = User?.AuthToken
            });

            await File.WriteAllTextAsync(Path.Join(path, "userinfo"), userinfo);
            Process.Start(Path.Join(path, "RxGame-Win64-Test.exe"), $"-ini:RxEngine:MotigaAuthIntegration.AuthUrlPrefix={config.Host}/,ArcIntegration.AuthUrlPrefix={config.Host}/ -log=GiganticEmu.Launcher.{User!.UserName}.log");
        }
    }
}
