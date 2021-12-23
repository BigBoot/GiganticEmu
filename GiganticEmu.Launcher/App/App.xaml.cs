using CommandLine;
using GiganticEmu.Shared;
using ReactiveUI;
using Refit;
using Splat;
using System;
using System.Net.Http;
using System.Reflection;
using System.Windows;

namespace GiganticEmu.Launcher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string DEFAULT_HOST = "https://api.mistforge.net";

    public class Options
    {
        [Option('h', "host", Required = false, HelpText = "Connect to the specified backend server.", Default = DEFAULT_HOST)]
        public string Host { get; set; } = default!;

        [Option('g', "game", Required = false, HelpText = "The path to the game.", Default = ".")]
        public string Game { get; set; } = default!;
    }

    public App()
    {
        Settings.Load();

        Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs())
            .WithParsed<Options>(o =>
            {
                Locator.CurrentMutable.RegisterLazySingleton(() => new LauncherConfiguration()
                {
                    Host = o.Host,
                    Game = o.Game
                });
            });

        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());

        Locator.CurrentMutable.RegisterLazySingleton(() => new CredentialStorage());
        Locator.CurrentMutable.RegisterLazySingleton(() => new UserManager());
        Locator.CurrentMutable.RegisterLazySingleton(() => new ApiTokenHandler());
        Locator.CurrentMutable.RegisterLazySingleton(() => new GitHub());
        Locator.CurrentMutable.RegisterLazySingleton(() => RestService.For<IBackendApi>(new HttpClient(Locator.Current.GetService<ApiTokenHandler>()!)
        {
            BaseAddress = new Uri(Locator.Current.GetService<LauncherConfiguration>()!.Host)
        }), typeof(IBackendApi));
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        Settings.Save();
    }
}
