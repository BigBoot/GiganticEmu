using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;

public class MiceClient
{
    const int MIN_BUFFER_SIZE = 512;
    const int MAX_LENGTH_BYTES = sizeof(long) + 1;
    private TcpClient _tcp;
    private NetworkStream _tcpStream;
    private Pipe _pipe = new Pipe();
    private bool _authenticated = false;
    private bool _closed = false;
    private static Dictionary<String, Func<dynamic, Task<object>>> HANDLERS = Assembly.GetEntryAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<MiceCommandAttribute>(false).Any())
            .ToDictionary(m => m.GetCustomAttribute<MiceCommandAttribute>(false).Command, m => (Func<dynamic, Task<object>>)Delegate.CreateDelegate(typeof(Func<dynamic, Task<object>>), null, m));

    private CancellationTokenSource _cts = new CancellationTokenSource();

    private Salsa _salsaIn;
    private Salsa _salsaOut;

    private string _username;

    public MiceClient(TcpClient tcp, string username)
    {
        _tcp = tcp;
        _tcpStream = _tcp.GetStream();

        _salsaIn = new Salsa("bbbbbbbbbbbbbbbb", 16);
        _salsaOut = new Salsa("bbbbbbbbbbbbbbbb", 16);

        _username = username;
    }

    public async Task Run()
    {
        Task receiving = ReceiveTask();
        Task reading = ReadTask();
        // Task match = Task.Delay(5000).ContinueWith(async _ =>
        // {
        //     Console.WriteLine("Sending [match.ready]");
        //     var ck2 = Convert.ToBase64String(Encoding.UTF8.GetBytes("imagoodcipherkey"));
        //     var ck = Convert.ToBase64String(Encoding.UTF8.GetBytes("amotigadeveloper"));
        //     var bcryptHmac = Convert.ToBase64String(Encoding.UTF8.GetBytes("totsagoodsuperlonghmacsecretkeys"));
        //     var msg = new object[] { "match.ready", new
        //     {
        //         matchinfo = new
        //         {
        //             server = new
        //             {
        //                 connstr = "127.0.0.1:7777",
        //                 map = "lv_canyon",
        //             },
        //             instanceid = "12",
        //             token = ck + ck2 + bcryptHmac,
        //             meta = new
        //             {
        //                 moid = 2,
        //             },
        //         },
        //     }}.ToJson();
        //     await SendResponse(msg);
        // });

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
                // Console.WriteLine("Receiving...");
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
                await Console.Error.WriteLineAsync(ex.ToString());
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
                    //Console.WriteLine($"Received command (len: {cmdLen})");
                    await HandleCommand(buffer.Slice(buffer.Start, cmdLen).ToArray());
                    pipeReader.AdvanceTo(buffer.GetPosition(cmdLen));
                    cmdLen = 0;
                    cmdReady = false;
                }
                else
                {
                    pipeReader.AdvanceTo(buffer.Start, buffer.End);
                    // Console.WriteLine("Waiting for more data!");
                }
            }
            catch (OperationCanceledException)
            {
                // Client requested disconnected
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.ToString());
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

    private async Task SendResponse(string response)
    {
        var encrypted = _salsaOut.Encrypt(response);
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
            Console.WriteLine(ex.ToString());
            throw;
        }

        var (cmd, payload, id) = (msg[0], msg[1], msg[2]);

        if (cmd == ".close" || cmd == "party.leave")
        {
            Console.WriteLine($"Received .close command!");
            _closed = true;
            return;
        }

        if (HANDLERS.ContainsKey(cmd))
        {
            Console.WriteLine($"Received handled command: {cmd}");
            try
            {
                var response = await HANDLERS[cmd](payload);
                if (response != null)
                {
                    var responseMsg = new[] { new[] { response }, id }.ToJson();
                    // Console.WriteLine(responseMsg);
                    await SendResponse(responseMsg);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
        else
        {
            Console.WriteLine($"Received unhandled command: {cmd} \n{((object)payload).ToJson()}");
            // Console.WriteLine($"Received unhandled command: {cmd}");
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
                name = _username,
                deviceid = "noString",
                gameid = "ggc",
                version = "298288",
                xmpp = new
                {
                    host = "127.0.0.1",
                },
            },
        }.ToJson();

        await SendResponse(response);

        _authenticated = true;

        Console.WriteLine("Client authenticated!");
    }
}