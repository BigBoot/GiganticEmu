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
        GameLauncher.StartGame(User?.UserName, User?.UserName, User?.AuthToken);
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

            GameLauncher.StartServer(_serverPort.Value);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = $"http://127.0.0.1:{_serverPort}/",
            UseShellExecute = true,
        });
    }
}
