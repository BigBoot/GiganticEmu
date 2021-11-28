using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GiganticEmu.Web;

class Program
{
    static async Task Main(string[] args)
    {
        JsonExtensionMethods.DefaultOptions.WriteIndented = false;
        await CreateHostBuilder(new string[] { }).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => builder.AddBackendAppConfiguration(args))
            .ConfigureHostConfiguration(builder => builder.AddBackendHostConfiguration(args))
            .AddBackendLogging()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var hostConfig = new ConfigurationBuilder().AddBackendHostConfiguration(args).Build();
                var emuConfiguration = new BackendConfiguration();
                hostConfig.GetSection(BackendConfiguration.GiganticEmu).Bind(emuConfiguration);
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls($"http://{emuConfiguration.BindInterface}:{emuConfiguration.WebPort}/");
            });
    }
}
