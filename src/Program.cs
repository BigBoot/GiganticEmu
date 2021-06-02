using System;
using System.IO;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;

class Program
{
    private static async Task RunAsync()
    {
        var tcs = new TaskCompletionSource<object>();

        Console.CancelKeyPress += (sender, e) => { tcs.SetResult(null); };

        var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "username.txt");
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, "TheLegend27");
        }

        var username = await File.ReadAllTextAsync(path);

        var web = new HttpServer(port: 3000);
        var webTask = Task.Run(() => web.Start());
        var mice = new MiceServer(port: 4000, username);
        var miceTask = Task.Run(() => mice.Start());

        await Console.Out.WriteLineAsync($"Web Server listening on http://localhost:{web.Port}");
        await Console.Out.WriteLineAsync($"MICE Server listening on localhost:{mice.Port}");
        await Console.Out.WriteLineAsync("Press Ctrl+C to stop the server...");

        await tcs.Task;
        await mice.Stop();
        await web.Stop();
    }

    static void Main(string[] args)
    {
        JsonExtensionMethods.DefaultOptions.Converters.Add(new DynamicJsonConverter());
        JsonExtensionMethods.DefaultOptions.WriteIndented = false;

        RunAsync().GetAwaiter().GetResult();
    }
}
