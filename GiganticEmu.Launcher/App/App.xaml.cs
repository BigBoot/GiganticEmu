using GiganticEmu.Shared;
using ReactiveUI;
using Refit;
using Splat;
using System;
using System.Net.Http;
using System.Reflection;
using System.Windows;

namespace GiganticEmu.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string HOST = "https://api.mistforge.net";
        //private const string HOST = "http://localhost:3000";

        public App()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());

            Locator.CurrentMutable.RegisterLazySingleton(() => new CredentialStorage());
            Locator.CurrentMutable.RegisterLazySingleton(() => new UserManager());
            Locator.CurrentMutable.RegisterLazySingleton(() => new ApiTokenHandler());
            Locator.CurrentMutable.RegisterLazySingleton(() => RestService.For<IBackendApi>(new HttpClient(Locator.Current.GetService<ApiTokenHandler>()!) { BaseAddress = new Uri(HOST) }), typeof(IBackendApi));
        }
    }
}
