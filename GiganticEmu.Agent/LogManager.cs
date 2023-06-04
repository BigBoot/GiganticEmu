using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GiganticEmu.Shared;

namespace GiganticEmu.Agent;

public class LogManager
{
    private static Regex RE_PLAYER_JOIN_TOKEN = new Regex(@"Join request: :\d+\/MainMenu\?Name=(?<PlayerName>.+?)\?MatchToken=(?<MatchToken>.+?)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
    private static Regex RE_TEAM_SIZE = new Regex(@"Rx_General: TeamSize:  (?<size>\d+)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
    private static Regex RE_TEAM_PLAYER = new Regex(@"Rx_General: -- (?<name>.+?) \((?<id>\d+)\)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
    private static Regex RE_WINNER = new Regex($@"(?<=ScriptLog: \(RXPAWN_)(?<entity>{String.Join("|", Guardian.ALL_GUARDIANS.Select(x => x.CodeName))})(?=_CONTENT_0\) RXPAWN::DYING:DESTROYED)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private async Task<string> GetLogFile(int port)
    {
        string basePath;

        if (PlatformUtils.IsWindows)
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        else
        {
            var windowsPath = await Process.Start(new ProcessStartInfo
            {
                FileName = "wine",
                ArgumentList = { "cmd", "/C", "echo %USERPROFILE%\\Documents" },
                UseShellExecute = false,
                RedirectStandardOutput = true
            })!.StandardOutput.ReadToEndAsync()!;

            var unixPath = await Process.Start(new ProcessStartInfo
            {
                FileName = "winepath",
                ArgumentList = { "-u", windowsPath.Trim() },
                UseShellExecute = false,
                RedirectStandardOutput = true,
            })!.StandardOutput.ReadToEndAsync();

            basePath = unixPath.Trim();
        }

        return Path.GetFullPath(Path.Join(basePath, "My Games", "Gigantic", "RxGame", "Logs", $"GiganticEmu.Agent.{port}.log"));
    }

    public async Task<MatchResult> GetMatchResult(int port)
    {
        var logFile = await GetLogFile(port);

        var teams = new List<List<Player>>();
        MatchResult.Team? winner = null;
        var tokens = new Dictionary<string, string>();

        using (var stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = (await reader.ReadLineAsync())!;

                    if (RE_PLAYER_JOIN_TOKEN.IsMatch(line))
                    {
                        var match = RE_PLAYER_JOIN_TOKEN.Match(line);
                        tokens[match.Groups["PlayerName"].Value] = match.Groups["MatchToken"].Value;
                    }

                    if (RE_TEAM_SIZE.IsMatch(line))
                    {
                        var match = RE_TEAM_SIZE.Match(line);
                        var players = await GetTeamPlayers(reader, int.Parse(match.Groups["size"].Value)).ToListAsync();
                        teams.Add(players);
                    }

                    if (RE_WINNER.IsMatch(line))
                    {
                        var match = RE_WINNER.Match(line);
                        winner = match.Groups["entity"].Value.ToLowerInvariant() switch
                        {
                            "griffin" => MatchResult.Team.Team2,
                            "naga" => MatchResult.Team.Team1,
                            _ => null,
                        };
                    }
                }
            }
        }

        if (teams.Count != 2)
        {
            throw new LogParseException($"Expected 2 teams but found {teams.Count} teams.");
        }

        var duplicatePlayers = teams
            .SelectMany(x => x)
            .GroupBy(x => x.Name)
            .Where(x => x.Count() != 1);

        if (duplicatePlayers.Count() > 0)
        {
            throw new LogParseException($"Expected to find no duplicate players but found duplicate players:\n"
                + String.Join("\n", duplicatePlayers.Select(x => $"{x.Key} ({x.Count()})").ToString()));
        }

        foreach (var (name, token) in tokens)
        {
            var player = teams.SelectMany(x => x).FirstOrDefault(x => x.Name == name);
            if (player == null)
            {
                throw new LogParseException($"Got token for player {name} but player does not appear to be in match...");
            }

            player.MatchToken = token;
        }

        return new MatchResult
        {
            Team1 = teams[0],
            Team2 = teams[1],
            Winner = winner,
        };
    }

    private async IAsyncEnumerable<Player> GetTeamPlayers(StreamReader reader, int players)
    {
        var count = 0;

        while (count < players)
        {
            var line = await reader.ReadLineAsync();

            if (line == null)
            {
                break;
            }

            var match = RE_TEAM_PLAYER.Match(line);
            if (match.Success)
            {
                yield return new Player
                {
                    Name = match.Groups["name"].Value,
                    MotigaId = int.Parse(match.Groups["id"].Value),
                };
                count++;
            }
        }
        if (count != players)
        {
            throw new LogParseException($"Expected to find {players} players but reached EoF after {count} players.");
        }
    }

    public async Task DeleteLog(int port)
    {
        var logFile = await GetLogFile(port);
        try
        {
            File.Delete(logFile);
        }
        catch (Exception)
        {

        }
    }
}