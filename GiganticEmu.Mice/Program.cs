using System;
using System.IO;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Mice;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        .ConfigureHostConfiguration(configHost =>
        {
            configHost.SetBasePath(Directory.GetCurrentDirectory());
            configHost.AddJsonFile("hostsettings.json", optional: true);
            configHost.AddEnvironmentVariables();
            configHost.AddCommandLine(args);
        })
        .ConfigureAppConfiguration((hostContext, configApp) =>
        {
            configApp.AddJsonFile("appsettings.json", optional: true);
            configApp.AddJsonFile(
                $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                optional: true);
            configApp.AddEnvironmentVariables();
            configApp.AddCommandLine(args);
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.Configure<GiganticEmuConfiguration>(hostContext.Configuration.GetSection(GiganticEmuConfiguration.GiganticEmu));
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

