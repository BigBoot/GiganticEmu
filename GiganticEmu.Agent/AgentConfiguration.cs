using Microsoft.Extensions.Configuration;

namespace GiganticEmu.Agent
{
    public class AgentConfiguration
    {
        public string ApiKey { get; set; } = default!;
        public string BindInterface { get; set; } = "0.0.0.0";
        public string? GiganticPath { get; set; }
        public int MaxInstances { get; set; } = 1;
        public int ServerPort { get; set; } = 7777;
        public int WebPort { get; set; } = 8080;

        #region GCP config.json compatibility
        private string api_key
        {
            get => default!;
            set => ApiKey = value;
        }
        private string? gigantic_path
        {
            get => default;
            set => GiganticPath = value;
        }
        private int http_port
        {
            get => default;
            set => WebPort = value;
        }
        private int max_instances
        {
            get => default;
            set => MaxInstances = value;
        }
        private int server_port
        {
            get => default;
            set => ServerPort = value;
        }
        #endregion
    }

    public static class AgentConfigurationExtensions
    {
        public static IConfigurationBuilder AddAgentConfiguration(this IConfigurationBuilder config, string[] args)
        {
            config.AddJsonFile("config.json", optional: true);
            config.AddJsonFile("GiganticEmu.json", optional: true);
            config.AddJsonFile("GiganticEmu.Agent.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);

            return config;
        }
    }
}