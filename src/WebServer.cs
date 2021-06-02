using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Reflection;

class HttpServer
{
    public uint Port { get; private set; }

    private readonly HttpListener _listener;

    public HttpServer(uint port)
    {
        Port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    }

    public async Task Start()
    {
        _listener.Start();

        while (true)
        {
            var ctx = await _listener.GetContextAsync();

            Console.Out.WriteLine("Serving file: '{0}'", ctx.Request.Url);

            Func<HttpListenerContext, Task<string>> handler = null;

            switch (ctx.Request.Url.AbsolutePath)
            {
                case "/auth/0.0/arc/auth":
                    handler = Auth;
                    break;

                case "/cdn":
                    handler = Cdn;
                    break;
            }

            if (handler != null)
            {
                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    await sw.WriteAsync(await handler(ctx));
                    await sw.FlushAsync();
                }
            }
        }
    }

    private async Task<string> Auth(HttpListenerContext ctx)
    {
        ctx.Response.Headers.Add("content-type: application/json");

        var version = 16897;
        var token = "zwl42ixhzshhfajvt8likv8ujkyoxlrn";
        var http_host = "127.0.0.1";
        var http_port = 3000;
        var mice_host = "127.0.0.1";
        var mice_port = 4000;
        var SALSA_CK = "aaaaaaaaaaaaaaaa";
        var SALSA_SCK = "bbbbbbbbbbbbbbbb";

        return JsonSerializer.Serialize(new
        {
            result = "ok",
            auth = token,
            token = token,
            name = "Player",
            username = "test@example.de",
            buddy_key = false,
            founders_pack = true,

            host = mice_host,
            port = mice_port,
            ck = Convert.ToBase64String(new byte[] { 0, 0 }.Concat(Encoding.UTF8.GetBytes(SALSA_CK)).ToArray()),
            sck = Convert.ToBase64String(new byte[] { 0, 0 }.Concat(Encoding.UTF8.GetBytes(SALSA_SCK)).ToArray()),

            flags = "", // unknown string
            xbox_preview = false,
            accounts = "accmple", // unknown string
            mostash_verbosity_level = 0,
            min_version = version,
            current_version = version, // game doesnt event check
            catalog = new
            {

                cdn_url = $"http://{http_host}:{http_port}/cdn",
                sha256_digest = "53e8adf03a506dedde073150537b9bd79168ae7e30b3b2152835c22abf98455b",
            },
            announcements = new
            {

                message = "serverMessage",
                status = "serverStatus",
            },
            voice_chat = new
            {
                baseurl = "http://127.0.01/voice.html",
                username = "sup:.username.@voice.sipServ.com",
                token = "sipToken"
            },
        }).ToString();
    }

    private async Task<string> Cdn(HttpListenerContext ctx)
    {
        ctx.Response.Headers.Add("content-type: application/json");

        var assembly = Assembly.GetEntryAssembly();

        using (var input = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.resources.cdn.json"))
        {
            var buffer = new byte[input.Length];
            await input.ReadAsync(buffer, 0, (int)input.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }

    public async Task Stop()
    {
        await Console.Out.WriteLineAsync(
            "Stopping server...");

        if (_listener.IsListening)
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
