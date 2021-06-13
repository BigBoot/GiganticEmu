using System.IO;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GiganticEmu.Agent
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddAgentConfiguration(args)
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .Build();
                    var agentConfiguration = new AgentConfiguration();
                    configuration.Bind(agentConfiguration, o => o.BindNonPublicProperties = true);

                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://{agentConfiguration.BindInterface}:{agentConfiguration.WebPort}/");
                });
        }
    }
}
