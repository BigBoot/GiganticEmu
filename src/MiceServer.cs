using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

class MiceServer
{
    public uint Port { get; }

    private readonly TcpListener _listener;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly string _username;


    public MiceServer(uint port, string username)
    {
        Port = port;
        _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), (int)port);
        _username = username;
    }

    public async Task Start()
    {
        _listener.Start();

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();

            await Console.Out.WriteLineAsync("Mice Client connected...");
            var _ = Task.Run(async () => await new MiceClient(client, _username).Run());
        }
    }

    public async Task Stop()
    {
        await Console.Out.WriteLineAsync("Stopping mice server...");

        _cts.Cancel();
        _listener.Stop();
    }
}
