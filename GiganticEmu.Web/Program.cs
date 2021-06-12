using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Shared.Backend;
using GiganticEmu.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
        var hostConfig = new ConfigurationBuilder().AddGiganticEmuHostConfiguration(args).Build();
        var emuConfiguration = new GiganticEmuConfiguration();
        hostConfig.GetSection(GiganticEmuConfiguration.GiganticEmu).Bind(emuConfiguration);
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseConfiguration(hostConfig);
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls($"http://{emuConfiguration.BindInterface}:{emuConfiguration.WebPort}/");
            });
    }

}
