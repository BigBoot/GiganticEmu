using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace GiganticEmu.Shared.Backend
{
    public class BackendConfiguration
    {
        public const string GiganticEmu = "GiganticEmu";

        public class Agent
        {
            public string Host { get; set; } = default!;
            public string Region { get; set; } = "NA";
            public string ApiKey { get; set; } = default!;
        }

        public string SalsaCK { get; set; } = default!;
        public string BindInterface { get; set; } = "127.0.0.1";
        public int WebPort { get; set; } = 3000;
        public int MicePort { get; set; } = 4000;
        public string MiceHost { get; set; } = "localhost";
        public int GameVersion { get; set; } = 301530;
        public ICollection<Agent> Agents { get; set; } = new List<Agent>();
        public int NumPlayers { get; set; } = 10;
        public string? ApiKey { get; set; }
    }

    public static class BackendConfigurationExtensions
    {
        public static IConfigurationBuilder AddBackendHostConfiguration(this IConfigurationBuilder config, string[] args)
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("hostsettings.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);

            return config;
        }
        public static IConfigurationBuilder AddBackendAppConfiguration(this IConfigurationBuilder config, string[] args)
        {
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);

            return config;
        }
    }
}