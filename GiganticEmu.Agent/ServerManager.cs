using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using GiganticEmu.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace GiganticEmu.Agent;

public class ServerManager
{
    private record Instance(Process Process, string AdminPassword, string DefaultGameIniPath, int Port);

    private static Regex RE_CREATURE = new Regex(@"DefaultMinionLoadout\[(\d+)]=""\w*""", RegexOptions.Compiled | RegexOptions.Multiline);
    private static Regex RE_MAX_PLAYERS = new Regex(@"MaxPlayers=\d*", RegexOptions.Compiled | RegexOptions.Multiline);
    private static Regex RE_ADMIN_PASSWORD = new Regex(@"AdminPassword=\w*", RegexOptions.Compiled | RegexOptions.Multiline);

    private static Random RANDOM = new Random();

    private readonly ILogger<ServerManager> _logger;
    private readonly AgentConfiguration _configuration;
    private readonly LogManager _logManager;
    private readonly PriorityQueue<int, int> _freePorts;
    private readonly IDictionary<int, Instance> _instances;
    private readonly string _gamePath;
    private readonly string _binaryPath;
    private readonly string _configPath;
    private readonly string _instanceConfigPath;

    public int RunningInstances { get => _configuration.MaxInstances - _freePorts.Count; }

    public ServerManager(ILogger<ServerManager> logger, IOptions<AgentConfiguration> configuration, LogManager logManager)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _logManager = logManager;
        _freePorts = new PriorityQueue<int, int>(Enumerable.Range(_configuration.ServerPort, _configuration.MaxInstances).Select(i => (i, i)));
        _instances = new Dictionary<int, Instance>();
        _gamePath = _configuration.GiganticPath ?? Directory.GetCurrentDirectory();
        _binaryPath = Path.GetFullPath(Path.Join(_gamePath, "Binaries", "Win64"));
        _configPath = Path.GetFullPath(Path.Join(_gamePath, "RxGame", "Config"));
        _instanceConfigPath = _configuration.InstanceConfigPath ?? _configPath;
    }

    public async Task<int> StartInstance(string map, int? maxPlayers = null, (string, string, string)? creatures = null, bool useLobby = false, string? reportUrl = null)
    {
        if (!_freePorts.TryDequeue(out int port, out _))
        {
            throw new NoInstanceAvailableException();
        }

        Instance instance;
        try
        {
            instance = await StartInstance(port, map, maxPlayers, creatures, useLobby);
        }
        catch (Exception)
        {
            _instances.Remove(port);
            _freePorts.Enqueue(port, port);

            throw;
        }

        if (instance != null)
        {
            _ = Task
                .Run(async () => RunInstance(instance, reportUrl))
                .LogExceptions(_logger);
        }

        return port;
    }

    private async Task<Instance> StartInstance(int port, string map, int? maxPlayers, (string, string, string)? creatures, bool useLobby)
    {
        useLobby = false;

        string? binary = null;
        foreach (var exe in new[] { "RxGame-Win64-Test.exe", "RxGame-Arc-Win64-Test.exe" })
        {
            if (File.Exists(Path.Join(_binaryPath, exe)))
            {
                binary = Path.Join(_binaryPath, exe);
                break;
            }
        }

        if (binary == null)
        {
            throw new UnableToStartServerException("Unable to find game exe, please make sure the path in your configuration file is correct...");
        }

        var process = new Process();

        if (PlatformUtils.IsWindows)
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

            process.StartInfo.ArgumentList.Add(binary);
        }

        process.StartInfo.ArgumentList.Add($"server");

        process.StartInfo.ArgumentList.Add((useLobby ? "lobby" : map) + "?listen=true");

        process.StartInfo.ArgumentList.Add($"-port={port}");
        process.StartInfo.ArgumentList.Add($"-log=GiganticEmu.Agent.{port}.log");
        process.StartInfo.ArgumentList.Add($"-forcelogflush");

        var defGameIni = await File.ReadAllTextAsync(Path.Join(_configPath, "DefaultGame.ini"));
        var defGameIniPath = Path.GetFullPath(Path.Join(_instanceConfigPath, $"_DefaultGame_{port}.ini"));

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
        if (PlatformUtils.IsWindows)
        {
            process.StartInfo.ArgumentList.Add($"-defgameini={defGameIniPath}");
        }
        else
        {
            process.StartInfo.ArgumentList.Add($"-defgameini=Z:{defGameIniPath.Replace('/', '\\')}");
        }

        await _logManager.DeleteLog(port);

        process.Start();
        if (PlatformUtils.IsWindows)
        {
            ChildProcessTracker.AddProcess(process);
        }

        var instance = new Instance(process, adminPassword, defGameIniPath, port);
        _instances.Add(port, instance);
        return instance;
    }

    private async Task RunInstance(Instance instance, string? reportUrl)
    {
        try
        {
            var timeout = Task.Delay(TimeSpan.FromHours(2));
            if (await Task.WhenAny(instance.Process.WaitForExitAsync(), timeout) == timeout)
            {
                instance.Process.Kill();
                await instance.Process.WaitForExitAsync();
            }

            if (reportUrl != null)
            {
                try
                {
                    var result = await _logManager.GetMatchResult(instance.Port);
                    await reportUrl
                        .WithPolly(policy => policy.RetryAsync(3))
                        .PostJsonAsync(new ReportPostRequest
                        {
                            Result = result,
                            Server = _configuration.Title,
                        })
                        .ReceiveJson<ReportPostResponse>();
                }
                catch (LogParseException e)
                {
                    await reportUrl
                        .WithPolly(policy => policy.RetryAsync(3))
                        .PostJsonAsync(new ReportPostRequest
                        {
                            ParseError = e.Message,
                            Server = _configuration.Title,
                        })
                        .ReceiveJson<ReportPostResponse>();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
        }
        finally
        {
            _instances.Remove(instance.Port);
            _freePorts.Enqueue(instance.Port, instance.Port);
            File.Delete(instance.DefaultGameIniPath);
        }
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
