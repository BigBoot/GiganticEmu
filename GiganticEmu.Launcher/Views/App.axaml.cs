using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommandLine;
using Flurl.Http;
using Flurl.Serialization.TextJson;
using GiganticEmu.Shared;
using Material.Styles.Themes;
using Microsoft.Extensions.Caching.Memory;
using Polly.Caching.Memory;
using ReactiveUI;
using Refit;
using Splat;

namespace GiganticEmu.Launcher
{
    public partial class App : Application
    {
        private const string DEFAULT_HOST = "https://api.mistforge.net";

        public class Options
        {
            [Option('h', "host", Required = false, HelpText = "Connect to the specified backend server.",
                Default = DEFAULT_HOST)]
            public string Host { get; set; } = default!;

            [Option('g', "game", Required = false, HelpText = "The path to the game.", Default = ".")]
            public string Game { get; set; } = default!;
        }

        public override void Initialize()
        {
            SetupServices();
            HandleCmdLine();

            AvaloniaXamlLoader.Load(this);

            var palette = new PaletteHelper();
            var theme = palette.GetTheme();
            theme.SetPrimaryColor(Color.Parse("#1a7d93"));
            palette.SetTheme(theme);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void SetupServices()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());

            Locator.CurrentMutable.RegisterLazySingleton<IAssetLoader>(() => new AvaloniaAssetLoader());

            Locator.CurrentMutable.RegisterLazySingleton(() => new ApiTokenHandler());
            Locator.CurrentMutable.RegisterLazySingleton(() => new AutoUpdater());
            Locator.CurrentMutable.RegisterLazySingleton(() => new CredentialStorage());
            Locator.CurrentMutable.RegisterLazySingleton(() => new GameLauncher());
            Locator.CurrentMutable.RegisterLazySingleton(() => new GitHub());
            Locator.CurrentMutable.RegisterLazySingleton(() => new Settings());
            Locator.CurrentMutable.RegisterLazySingleton(() => new UserManager());

            Locator.CurrentMutable.RegisterLazySingleton(() => RestService.For<IBackendApi>(
                new HttpClient(Locator.Current.RequireService<ApiTokenHandler>())
                {
                    BaseAddress = new Uri(Locator.Current.RequireService<LauncherConfiguration>().Host)
                }), typeof(IBackendApi));

            Task.Run(async () => Locator.Current.RequireService<Settings>().Load()).Wait();
        }

        private void HandleCmdLine()
        {
            new Parser(with =>
                with.IgnoreUnknownArguments = true
            ).ParseArguments<Options>(Environment.GetCommandLineArgs())
                .WithParsed<Options>(o =>
                {
                    Locator.CurrentMutable.RegisterLazySingleton(() => new LauncherConfiguration()
                    {
                        Host = o.Host,
                        Game = o.Game
                    });
                });
        }
    }
}