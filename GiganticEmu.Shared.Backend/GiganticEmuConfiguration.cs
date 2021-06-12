using System.IO;
using Microsoft.Extensions.Configuration;

namespace GiganticEmu.Shared.Backend
{
    public class GiganticEmuConfiguration
    {
        public const string GiganticEmu = "GiganticEmu";

        public string SalsaCK { get; set; } = default!;
        public string BindInterface { get; set; } = "127.0.0.1";
        public int WebPort { get; set; } = 3000;
        public int MicePort { get; set; } = 4000;
        public string MiceHost { get; set; } = "localhost";
        public int GameVersion { get; set; } = 301530;
    }

    public static class GiganticEmuConfigurationExtensions
    {
        public static IConfigurationBuilder AddGiganticEmuHostConfiguration(this IConfigurationBuilder config, string[] args)
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("hostsettings.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);

            return config;
        }
        public static IConfigurationBuilder AddGiganticEmuAppConfiguration(this IConfigurationBuilder config, string[] args)
        {
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);

            return config;
        }
    }
}