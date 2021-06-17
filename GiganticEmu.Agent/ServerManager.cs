using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiganticEmu.Agent
{
    public class ServerManager
    {
        private readonly ILogger<ServerManager> _logger;
        private readonly AgentConfiguration _configuration;
        private readonly Queue<int> _freePorts;
        private readonly string _gamePath;
        private readonly string _binaryPath;

        public ServerManager(ILogger<ServerManager> logger, IOptions<AgentConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _freePorts = new Queue<int>(Enumerable.Range(_configuration.ServerPort, _configuration.MaxInstances));
            _gamePath = _configuration.GiganticPath ?? Directory.GetCurrentDirectory();
            _binaryPath = Path.Join(_gamePath, "Binaries", "Win64");
        }

        public async Task<int> StartInstance(string map)
        {
            int port;
            if (!_freePorts.TryDequeue(out port))
            {
                return 0;
            }

            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Path.Join(_binaryPath, "RxGame-Win64-Test.exe"),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = _gamePath
            };

            process.StartInfo.ArgumentList.Add($"server");
            process.StartInfo.ArgumentList.Add($"lobby");
            process.StartInfo.ArgumentList.Add($"-port={port}");
            process.StartInfo.ArgumentList.Add($"-log=GiganticEmu.Agent.{port}.log");
            process.StartInfo.ArgumentList.Add($"-forcelogflush");


            // process.StartInfo.ArgumentList.Add($"-forcelogflush");
            // process.StartInfo.ArgumentList.Add($"RXRESERVATION=a_file");
            // process.StartInfo.ArgumentList.Add($"MAPNAME={map}");
            // process.StartInfo.ArgumentList.Add($"GAMESTATSURL=127.0.0.1:3000/some/fake/address");
            // process.StartInfo.ArgumentList.Add($"TEAMINFOJSONSTR={{}}");
            // process.StartInfo.ArgumentList.Add($"DATACENTERID=1");
            process.StartInfo.ArgumentList.Add($"-ini:RxGame:RxGame.RxPreGameLobbyGame_Base.ReservedMapName={map}");

            await File.WriteAllTextAsync(Path.Join(_binaryPath, "serverstart"), "");

            process.Start();
            ChildProcessTracker.AddProcess(process);

            _ = Task.Run(async () =>
            {
                await process.WaitForExitAsync();
                _freePorts.Enqueue(port);
            });

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromHours(1));
                process.Kill();
            });

            return port;
        }
    }
}