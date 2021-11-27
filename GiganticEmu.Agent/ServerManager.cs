using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiganticEmu.Agent
{
    public class ServerManager
    {
        private record Instance(Process Process, string AdminPassword);

        private static Regex RE_CREATURE = new Regex(@"DefaultMinionLoadout\[(\d+)]=""\w*""", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex RE_MAX_PLAYERS = new Regex(@"MaxPlayers=\d*", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex RE_ADMIN_PASSWORD = new Regex(@"AdminPassword=\w*", RegexOptions.Compiled | RegexOptions.Multiline);

        private static Random RANDOM = new Random();

        private readonly ILogger<ServerManager> _logger;
        private readonly AgentConfiguration _configuration;
        private readonly PriorityQueue<int, int> _freePorts;
        private readonly IDictionary<int, Instance> _instances;
        private readonly string _gamePath;
        private readonly string _binaryPath;
        private readonly string _configPath;

        public int RunningInstances { get => _instances.Count; }

        public ServerManager(ILogger<ServerManager> logger, IOptions<AgentConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _freePorts = new PriorityQueue<int, int>(Enumerable.Range(_configuration.ServerPort, _configuration.MaxInstances).Select(i => (i, i)));
            _instances = new Dictionary<int, Instance>();
            _gamePath = _configuration.GiganticPath ?? Directory.GetCurrentDirectory();
            _binaryPath = Path.GetFullPath(Path.Join(_gamePath, "Binaries", "Win64"));
            _configPath = Path.GetFullPath(Path.Join(_gamePath, "RxGame", "Config"));
        }

        public async Task<int> StartInstance(string map, int? maxPlayers = null, (string, string, string)? creatures = null, bool useLobby = false)
        {
            int port;
            if (!_freePorts.TryDequeue(out port, out _))
            {
                throw new NoInstanceAvailableException();
            }

            var process = new Process();


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Path.Join(_binaryPath, "RxGame-Win64-Test.exe"),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = _gamePath,
                };
            }
            else
            {
                var path = Environment.GetEnvironmentVariable("PATH");
                var _wine = _configuration.WinePath ?? path?.Split(":")
                    ?.Select(path => Path.GetFullPath(Path.Join(path, "wine")))
                    ?.FirstOrDefault(path => File.Exists(path));

                if (_wine == null)
                {
                    throw new UnableToStartServerException("Unable to find wine on PATH, please make sure wine is avaialble on PATH or add WinePath to your configuration file...");
                }

                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _wine,
                    WorkingDirectory = _gamePath,
                };

                process.StartInfo.ArgumentList.Add(Path.Join(_binaryPath, "RxGame-Win64-Test.exe"));
            }

            process.StartInfo.ArgumentList.Add($"server");

            process.StartInfo.ArgumentList.Add(useLobby ? "lobby" : map);

            process.StartInfo.ArgumentList.Add($"-port={port}");
            process.StartInfo.ArgumentList.Add($"-log=GiganticEmu.Agent.{port}.log");
            process.StartInfo.ArgumentList.Add($"-forcelogflush");

            var defGameIni = await File.ReadAllTextAsync(Path.Join(_configPath, "DefaultGame.ini"));
            var defGameIniPath = Path.GetFullPath(Path.Join(_configPath, $"_DefaultGame_{port}.ini"));

            var adminPassword = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 16)
                .Select(s => s[RANDOM.Next(s.Length)]).ToArray());

            defGameIni = RE_ADMIN_PASSWORD.Replace(defGameIni, $"AdminPassword={adminPassword}");

            if (useLobby)
            {
                process.StartInfo.ArgumentList.Add($"-ini:RxGame:RxGame.RxPreGameLobbyGame_Base.ReservedMapName={map}");
            }

            if (maxPlayers != null)
            {
                defGameIni = RE_MAX_PLAYERS.Replace(defGameIni, $"MaxPlayers={maxPlayers}");
            }

            if (creatures != null)
            {
                defGameIni = RE_CREATURE.Replace(defGameIni, match =>
                {
                    var n = int.Parse(match.Groups[1].Value);
                    var adult = n >= 3;
                    var id = (n % 3) switch
                    {
                        0 => creatures.Value.Item1,
                        1 => creatures.Value.Item2,
                        _ => creatures.Value.Item3,
                    };

                    var creature = Creature.ALL_CREATURES[id];
                    var minion = adult ? creature.Adult : creature.Baby;

                    return $"DefaultMinionLoadout[{n}]={minion}";
                });
            }

            await File.WriteAllTextAsync(defGameIniPath, defGameIni);
            process.StartInfo.ArgumentList.Add($"-defgameini={defGameIniPath}");

            process.Start();
            _instances.Add(port, new Instance(process, adminPassword));
            ChildProcessTracker.AddProcess(process);

            _ = Task.Run(async () =>
            {
                await process.WaitForExitAsync();
                _instances.Remove(port);
                _freePorts.Enqueue(port, port);
                File.Delete(defGameIniPath);
            });

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromHours(1));
                process.Kill();
            });

            return port;
        }

        public async Task KillInstance(int port)
        {
            try
            {
                _instances[port].Process.Kill();
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidInstanceException(port);
            }
        }

        public string GetAdminPassword(int port)
        {
            try
            {
                return _instances[port].AdminPassword;
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidInstanceException(port);
            }
        }
    }
}