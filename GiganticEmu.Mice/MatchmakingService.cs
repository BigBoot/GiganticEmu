using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace GiganticEmu.Mice;

public class MatchmakingService : IHostedService, IDisposable
{
    private readonly ILogger<MatchmakingService> _logger;
    private Timer _timer;
    private IDbContextFactory<ApplicationDatabase> _databaseFactory;
    private MiceServer _mice;
    private BackendConfiguration _configuration;
    private IList<(BackendConfiguration.AgentConfig, IAgentApi)> _agents;

    public record Session()
    {
        public IList<int> Players { get; init; } = default!;
        public int Size { get => Players.Count; }
    }

    public MatchmakingService(ILogger<MatchmakingService> logger, IDbContextFactory<ApplicationDatabase> databaseFactory, MiceServer mice, IOptions<BackendConfiguration> configuration)
    {
        _logger = logger;
        _timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
        _databaseFactory = databaseFactory;
        _mice = mice;
        _configuration = configuration.Value;
        _agents = _configuration.Agents.Select(agent => (agent, RestService.For<IAgentApi>(
            new HttpClient(
                new ApiTokenHandler
                {
                    AuthToken = agent.ApiKey
                })
            {
                BaseAddress = new Uri(agent.Host)
            })
        )).ToList();

    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is starting.", nameof(MatchmakingService));

        using (var database = _databaseFactory.CreateDbContext())
        {
            await database.Users
                .Where(user => user.InQueue)
                .ForEachAsync(user => user.InQueue = false);

            await database.SaveChangesAsync();
        }

        _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    private async void DoWork(object? state)
    {
        try
        {
            _logger.LogDebug("Looking for matches.", nameof(MatchmakingService));

            using var database = _databaseFactory.CreateDbContext();

            var queue = (await database.Users
                .Where(user => user.InQueue)
                .Select(user => new { SessionId = user.SessionId, MotigaId = user.MotigaId })
                .ToListAsync())
                .GroupBy(user => user.SessionId)
                .Select(group => new Session { Players = group.Select(user => user.MotigaId).ToList() })
                .ToList();

            var match = FindMatches(_configuration.NumPlayers, queue)
                .Select(groups => MakeTeams((_configuration.NumPlayers + 1) / 2, groups))
                .FirstOrDefault(x => x != null); // <<-- totally fair matchmaking

            _logger.LogInformation("{NumOfSessions} groups in queue.", queue.Count);
            //_logger.LogInformation(queue.ToJson());

            if (match is not null)
            {
                var map = _configuration.Maps.OrderBy(x => Guid.NewGuid()).First();

                _logger.LogInformation("Match found. Starting server with map {Map}", map);

                var server = await StartServer(map);

                if (server == null)
                {
                    _logger.LogError("No available instances found, trying later...");
                    return;
                }

                var players = match
                    .SelectMany((players, index) => players
                        .Select(player => (index, player)))
                    .ToDictionary(x => x.player, x => x.index);

                var clients = _mice.ConnectedClients.Where(client => players.ContainsKey(client.MotigaId));

                foreach (var player in players.Keys)
                {
                    var p = await database.Users.SingleAsync(user => user.MotigaId == player);
                    p.InQueue = false;
                }

                await database.SaveChangesAsync();

                _logger.LogInformation("Waiting for the server to become ready...");
                await Task.Delay(TimeSpan.FromSeconds(15));

                var ck2 = Convert.ToBase64String(Encoding.UTF8.GetBytes("imagoodcipherkey"));
                var ck = Convert.ToBase64String(Encoding.UTF8.GetBytes("amotigadeveloper"));
                var bcryptHmac = Convert.ToBase64String(Encoding.UTF8.GetBytes("totsagoodsuperlonghmacsecretkeys"));

                foreach (var client in clients)
                {
                    try
                    {
                        var team = players[client.MotigaId];
                        var msg = new object[] { "match.ready", new
                            {
                                matchinfo = new
                                {
                                    server = new
                                    {
                                        connstr = $"{server}?team=1",
                                        map = map,
                                    },
                                    instanceid = team.ToString(),
                                    token = ck + ck2 + bcryptHmac,
                                    meta = new
                                    {
                                        moid = 2,
                                    },
                                },
                            }};
                        await client.SendMessage(msg);
                    }
                    catch { }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during matchmaking");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is stopping.", nameof(MatchmakingService));

        _timer.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private List<List<Session>> FindMatches(int numPlayer, IList<Session> elements)
    {
        var result = new List<List<Session>>();
        FindMatchesRecursive(numPlayer, 0, result, new List<Session>(), new List<Session>(elements), 0);
        return result;
    }

    private void FindMatchesRecursive(int numPlayers, int currentNumPlayers, IList<List<Session>> result, IList<Session> included, IList<Session> notIncluded, int startIndex)
    {
        for (int index = startIndex; index < notIncluded.Count; index++)
        {
            var session = notIncluded[index];
            if (currentNumPlayers + session.Size == numPlayers)
            {
                var newResult = new List<Session>(included);
                newResult.Add(session);
                result.Add(newResult);
            }
            else if (currentNumPlayers + session.Size < numPlayers)
            {
                var nextIncluded = new List<Session>(included);
                nextIncluded.Add(session);
                var nextNotIncluded = new List<Session>(notIncluded);
                nextNotIncluded.Remove(session);
                FindMatchesRecursive(numPlayers, currentNumPlayers + session.Size, result, nextIncluded, nextNotIncluded, startIndex++);
            }
        }
    }

    private ICollection<ICollection<int>>? MakeTeams(int teamSize, IEnumerable<Session> sessions)
    {
        var available = new Queue<Session>(sessions.OrderByDescending(x => x.Size));

        var team1 = new List<int>();
        var team2 = new List<int>();
        while (available.Count > 0)
        {
            var next = available.Dequeue();
            if (team1.Count + next.Size == teamSize)
            {
                team1.AddRange(next.Players);
                team2.AddRange(available.SelectMany(group => group.Players));
                return new List<ICollection<int>> { team1, team2 };
            }

            if (team1.Count + next.Size < teamSize)
            {
                team1.AddRange(next.Players);
            }
            else
            {
                team2.AddRange(next.Players);
            }
        }

        return null;
    }

    private async Task<string?> StartServer(string map)
    {
        foreach (var (agent, api) in _agents)
        {
            try
            {
                var response = await api.StartServer(new ServerPostRequest
                {
                    Map = map
                });

                if (response.Code == RequestResult.Success && response.Port is int port)
                {
                    var url = new Uri(agent.Host);
                    var hostname = url.IdnHost;

                    IPAddress? ip;
                    if (!IPAddress.TryParse(hostname, out ip))
                    {
                        ip = (await Dns.GetHostAddressesAsync(hostname))
                            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                            .FirstOrDefault();
                    }

                    return $"{ip}:{port}";
                }
                else
                {
                    _logger.LogInformation("Unable to start instance on {Server}: {Reason}", agent.Host, response.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Unable to reach {Server}", agent.Host);
            }
        }
        return null;
    }
}
