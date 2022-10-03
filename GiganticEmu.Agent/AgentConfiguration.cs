using Microsoft.Extensions.Configuration;

namespace GiganticEmu.Agent;

public class AgentConfiguration
{
    public string ApiKey { get; set; } = default!;
    public string BindInterface { get; set; } = "0.0.0.0";
    public string? GiganticPath { get; set; }
    public int MaxInstances { get; set; } = 1;
    public string Title { get; set; } = "Gigantic Control Panel";
    public string ServerHost { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = 7777;
    public int WebPort { get; set; } = 8080;
    public string[] DefaultCreatures { get; set; } = new string[] { "bloomer", "cerb", "cyclops" };
    public string? WinePath { get; set; } = null;
    public string? InstanceConfigPath { get; set; } = null;
    public bool UseLobby { get; set; } = false;

    #region GCP config.json compatibility
    private string? api_key
    {
        get => default!;
        set { if (value != null) ApiKey = value; }
    }
    private string? gigantic_path
    {
        get => default;
        set { if (value != null) GiganticPath = value; }
    }
    private int? http_port
    {
        get => default;
        set { if (value.HasValue) WebPort = value.Value; }
    }
    private int? max_instances
    {
        get => default;
        set { if (value.HasValue) MaxInstances = value.Value; }
    }
    private int? server_port
    {
        get => default;
        set { if (value.HasValue) ServerPort = value.Value; }
    }
    private string? title
    {
        get => default!;
        set { if (value != null) Title = value; }
    }
    private string? server_url
    {
        get => default!;
        set { if (value != null) ServerHost = value; }
    }
    private string[]? default_creatures
    {
        get => default!;
        set { if (value != null) DefaultCreatures = value; }
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
        config.AddEnvironmentVariables(o =>
        {
            o.Prefix = "GiganticEmu_";
        });
        config.AddCommandLine(args);

        return config;
    }
}
