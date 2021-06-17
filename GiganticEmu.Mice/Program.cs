using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GiganticEmu.Mice
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonExtensionMethods.DefaultOptions.Converters.Add(new DynamicJsonConverter());
            JsonExtensionMethods.DefaultOptions.WriteIndented = false;

            Task.Run(async () => await CreateHostBuilder(args).Build().RunAsync())
                .GetAwaiter().GetResult();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .ConfigureHostConfiguration(configHost => configHost.AddBackendHostConfiguration(args))
            .ConfigureAppConfiguration((hostContext, configApp) => configApp.AddBackendAppConfiguration(args))
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<BackendConfiguration>(hostContext.Configuration.GetSection(BackendConfiguration.GiganticEmu));
                services.AddApplicationDatabase();

                services.AddMice();
            })
            .AddBackendLogging()
            .UseConsoleLifetime();
    }
}

