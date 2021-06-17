using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GiganticEmu.Web
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonExtensionMethods.DefaultOptions.WriteIndented = false;

            Task.Run(async () => await CreateHostBuilder(new string[] { }).Build().RunAsync())
                .GetAwaiter().GetResult();
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
}
