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

public class MiceServer : BackgroundService
{
    private readonly ILogger<MiceServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly GiganticEmuConfiguration _configuration;

    public MiceServer(ILogger<MiceServer> logger, IServiceProvider serviceProvider, IOptions<GiganticEmuConfiguration> configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), _configuration.MicePort);
        listener.Start();

        _logger.LogInformation("MICE Server listening on localhost:{_configuration.MicePort}", _configuration.MicePort);

        try
        {
            cancellationToken.Register(() => listener.Stop());
            while (listener.Server.IsBound)
            {
                var client = await listener.AcceptTcpClientAsync();

                _logger.LogInformation("Mice Client connected...");
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            await scope.ServiceProvider.GetRequiredService<MiceClient>().Run(client, cancellationToken);
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
