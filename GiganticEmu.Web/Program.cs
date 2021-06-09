using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

class Program
{
    static void Main(string[] args)
    {
        JsonExtensionMethods.DefaultOptions.WriteIndented = false;

        Task.Run(async () => await CreateHostBuilder(new string[] { }).Build().RunAsync())
            .GetAwaiter().GetResult();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://localhost:3000/");
            });
}
