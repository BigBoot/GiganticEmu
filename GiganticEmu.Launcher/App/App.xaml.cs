using CommandLine;
using GiganticEmu.Shared;
using Ookii.Dialogs.Wpf;
using ReactiveUI;
using Refit;
using Splat;
using System;
using System.Diagnostics;
using System.IO;
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

        [Option("update-target", Required = false, HelpText = "", Default = null, Hidden = true)]
        public string? UpdateTarget { get; set; }
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

                if (o.UpdateTarget is string updateTarget)
                {
                    var dlg = new ProgressDialog();

                    dlg.WindowTitle = "Mistforge Launcher Update";
                    dlg.Description = "Applying Update";
                    dlg.ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar;

                    dlg.Show();

                    var launcherLocation = Environment.ProcessPath!;

                    using (var input = File.OpenRead(launcherLocation))
                    using (var output = File.Create(updateTarget))
                    {
                        input.CopyTo(output);
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = updateTarget,
                        WorkingDirectory = new FileInfo(updateTarget).DirectoryName,
                    });
                    Application.Current.Shutdown(0);
                }
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
