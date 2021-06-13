using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                services.AddApplicationDatabase(c =>
                {
                    c.ConnectionString = hostContext.Configuration.GetConnectionString(name: "postgres");
                });

                services.AddMice();
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                configLogging.AddConsole();
                configLogging.AddDebug();
            })
            .UseConsoleLifetime();
    }
}

