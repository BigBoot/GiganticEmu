using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using GiganticEmu.Shared.Backend;
using Microsoft.Extensions.Logging;

public class MiceClient
{
    public ApplicationDatabase Database { get; init; }
    const int MIN_BUFFER_SIZE = 512;
    const int MAX_LENGTH_BYTES = sizeof(long) + 1;
    private TcpClient _tcp = null!;
    private NetworkStream _tcpStream = null!;
    private Pipe _pipe = new Pipe();
    private bool _authenticated = false;
    private bool _closed = false;
    private CancellationTokenSource _cts = null!;
    private Salsa _salsaIn = null!;
    private Salsa _salsaOut = null!;
    private ILogger<MiceClient> _logger;
    private MiceCommandHandler _commandHandler;
    private Guid id = Guid.NewGuid();

    public MiceClient(ILogger<MiceClient> logger, MiceCommandHandler commandHandler, ApplicationDatabase database)
    {
        _logger = logger;
        _commandHandler = commandHandler;
        Database = database;
    }

    public async Task Run(TcpClient tcp, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MiceGUID: {guid}", id);
        _tcp = tcp;
        _tcpStream = _tcp.GetStream();

        _salsaIn = new Salsa("bbbbbbbbbbbbbbbb", 16);
        _salsaOut = new Salsa("bbbbbbbbbbbbbbbb", 16);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task receiving = ReceiveTask();
        Task reading = ReadTask();

        await Task.WhenAll(reading, receiving);
        _tcp.Close();
    }

    private async Task ReceiveTask()
    {
        var writer = _pipe.Writer;
        while (!_closed)
        {
            Memory<byte> memory = writer.GetMemory(MIN_BUFFER_SIZE);
            try
            {
                int bytesRead = await _tcpStream.ReadAsync(memory, _cts.Token);
                _logger.LogDebug("Receiving...");
                if (bytesRead == 0)
                {
                    break;
                }
                // Tell the PipeWriter how much was read from the Socket
                writer.Advance(bytesRead);
            }
            catch (OperationCanceledException)
            {
                // Client requested disconnected
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during receive.");
                break;
            }

            // Make the data available to the PipeReader
            FlushResult result = await writer.FlushAsync();

            if (result.IsCompleted)
            {
                break;
            }
        }

        _cts.Cancel();

        // Tell the PipeReader that there's no more data coming
        writer.Complete();
    }

    private async Task ReadTask()
    {
        var pipeReader = _pipe.Reader;
        int cmdLen = 0;
        bool cmdReady = false;
        while (!_closed)
        {
            ReadResult result = await pipeReader.ReadAsync(_cts.Token);
            var buffer = result.Buffer;

            try
            {
                if (!cmdReady)
                {
                    cmdLen <<= 7;
                    var next = buffer.Slice(buffer.Start, 1).First.Span[0];
                    cmdLen += next & 0x7F;
                    if (next < 0x80)
                    {
                        cmdReady = true;
                    }

                    pipeReader.AdvanceTo(buffer.GetPosition(1, buffer.Start));
                }
                else if (buffer.Length >= cmdLen)
                {
                    _logger.LogDebug("Received command (len: {lentgh})", cmdLen);
                    await HandleCommand(buffer.Slice(buffer.Start, cmdLen).ToArray());
                    pipeReader.AdvanceTo(buffer.GetPosition(cmdLen));
                    cmdLen = 0;
                    cmdReady = false;
                }
                else
                {
                    pipeReader.AdvanceTo(buffer.Start, buffer.End);
                    _logger.LogDebug("Waiting for more data!");
                }
            }
            catch (OperationCanceledException)
            {
                // Client requested disconnected
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during read.");
                break;
            }

            // Stop reading if there's no more data coming
            if (result.IsCompleted)
            {
                break;
            }
        }

        _cts.Cancel();

        // Mark the PipeReader as complete
        _tcpStream.Dispose();
    }
    public async Task SendMessage(object response)
    {
        var json = response.ToJson();
        _logger.LogDebug(response.ToJson());
        var encrypted = _salsaOut.Encrypt(json);
        var length = EncodeSize(encrypted.Length);
        var packet = new byte[length.Length + encrypted.Length];
        System.Buffer.BlockCopy(length, 0, packet, 0, length.Length);
        System.Buffer.BlockCopy(encrypted, 0, packet, length.Length, encrypted.Length);
        await _tcpStream.WriteAsync(packet);
    }

    private byte[] EncodeSize(int size)
    {
        int length = size > 0 ? (int)Math.Floor(Math.Log(size) / Math.Log(0x80)) + 1 : 1;
        var data = new byte[length];

        if (length == 1)
            data[0] = (byte)size;
        else
        {
            int i = length - 2;
            data[length - 1] = (byte)(size & 0x7F);

            for (; i >= 0; i--)
            {
                size >>= 7;
                data[i] = (byte)((size & 0x7F) | 0x80);
            }
        }

        return data;
    }

    private async Task HandleCommand(byte[] data)
    {
        if (!_authenticated)
        {
            await Authenticate(data);
            return;
        }

        var content = _salsaIn.Decrypt(data);

        dynamic msg;
        try
        {
            msg = content.FromJsonTo<dynamic>();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Exception parsing json message.");
            throw;
        }

        var (cmd, payload, id) = (msg[0], msg[1], msg[2]);

        if (cmd == ".close" || cmd == "party.leave")
        {
            _logger.LogInformation("Received .close command!");
            _closed = true;
            return;
        }

        if (_commandHandler.CanHandle(cmd))
        {
            _logger.LogInformation("Received handled command: {cmd}", cmd as string);
            // _logger.LogDebug("{msg}", ((object)payload).ToJson());
            try
            {
                var response = await _commandHandler.Handle(cmd, payload, this);
                if (response != null)
                {
                    var responseMsg = new[] { new[] { response }, id };
                    await SendMessage(responseMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while handling command {cmd}.", cmd as string);
            }
        }
        else
        {
            _logger.LogInformation("Received unhandled command: {cmd}", cmd as string);
            //_logger.LogDebug("{msg}", ((object)payload).ToJson());
        }
    }

    private async Task Authenticate(byte[] cmd)
    {
        var msg = new Salsa("aaaaaaaaaaaaaaaa", 12).Decrypt(cmd);

        var response = new object[]
        {
            ".auth",
            new {
                time = 1,
                moid = 1,
                exp = 0,
                rank = 1,
                name = "",
                deviceid = "noString",
                gameid = "ggc",
                version = "298288",
                xmpp = new
                {
                    host = "127.0.0.1",
                },
            },
        };

        await SendMessage(response);

        _authenticated = true;

        _logger.LogInformation("Client authenticated!");
    }
}