using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using GiganticEmu.Mice;

public class MiceServer : BackgroundService
{
    public int Port { get => _options.Port; }
    public string Address { get => _options.Address; }
    private readonly ILogger<MiceServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MiceOptions _options;

    public MiceServer(ILogger<MiceServer> logger, IServiceProvider serviceProvider, MiceOptions options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(IPAddress.Parse(Address), Port);
        listener.Start();

        _logger.LogInformation("MICE Server listening on localhost:{Port}", Port);

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
