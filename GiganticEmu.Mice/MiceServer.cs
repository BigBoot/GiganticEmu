using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GiganticEmu.Shared.Backend;
using System.Collections.Generic;

public class MiceServer : BackgroundService
{
    public readonly IList<MiceClient> ConnectedClients = new List<MiceClient>();
    private readonly ILogger<MiceServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BackendConfiguration _configuration;

    public MiceServer(ILogger<MiceServer> logger, IServiceProvider serviceProvider, IOptions<BackendConfiguration> configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(IPAddress.Parse(_configuration.BindInterface), _configuration.MicePort);
        listener.Start();

        _logger.LogInformation("MICE Server listening on localhost:{_configuration.MicePort}", _configuration.MicePort);

        try
        {
            cancellationToken.Register(() => listener.Stop());
            while (listener.Server.IsBound)
            {
                var conn = await listener.AcceptTcpClientAsync();

                _logger.LogInformation("Mice Client connected...");
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var client = scope.ServiceProvider.GetRequiredService<MiceClient>();

                            ConnectedClients.Add(client);
                            client.ConnectionClosed += (sender, ev) =>
                            {
                                ConnectedClients.Remove(client);
                            };

                            await client.Run(conn, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "MiceClient exception");
                    }
                });

            }
        }
        finally
        {
            listener.Stop();
        }
    }
}
