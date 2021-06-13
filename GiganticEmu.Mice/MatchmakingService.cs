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

namespace GiganticEmu.Mice
{

    public class MatchmakingService : IHostedService, IDisposable
    {
        private readonly ILogger<MatchmakingService> _logger;
        private Timer _timer;
        private ApplicationDatabase _database;
        private MiceServer _mice;
        private BackendConfiguration _configuration;
        private IList<(BackendConfiguration.Agent, IAgentApi)> _agents;

        public record Session()
        {
            public IList<int> Players { get; init; } = default!;
            public int Size { get => Players.Count; }
        }

        public MatchmakingService(ILogger<MatchmakingService> logger, ApplicationDatabase database, MiceServer mice, IOptions<BackendConfiguration> configuration)
        {
            _logger = logger;
            _timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
            _database = database;
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
            _logger.LogInformation("{} is starting.", nameof(MatchmakingService));

            await _database.Users
                .Where(user => user.InQueue)
                .ForEachAsync(user => user.InQueue = false);

            await _database.SaveChangesAsync();

            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private async void DoWork(object? state)
        {
            try
            {
                _logger.LogDebug("{} matching triggered.", nameof(MatchmakingService));

                var queue = (await _database.Users
                    .Where(user => user.InQueue)
                    .Select(user => new { SessionId = user.SessionId, MotigaId = user.MotigaId })
                    .ToListAsync())
                    .GroupBy(user => user.SessionId)
                    .Select(group => new Session { Players = group.Select(user => user.MotigaId).ToList() })
                    .ToList();

                var matches = FindMatches(_configuration.NumPlayers, queue);

                var match = matches.FirstOrDefault(); // <<-- totally fair matchmaking

                if (match != null)
                {
                    _logger.LogInformation("{} match found.", nameof(MatchmakingService));

                    var map = new[] { "LV_Mistforge", "LV_Canyon", "LV_Valley" }.OrderBy(x => Guid.NewGuid()).First();

                    var server = await StartServer(map);

                    if (server == null)
                    {
                        _logger.LogError("No available instances found, trying later...");
                        return;
                    }

                    var players = match.SelectMany(session => session.Players).ToHashSet();
                    var clients = _mice.ConnectedClients.Where(client => players.Contains(client.MotigaId));

                    foreach (var player in players)
                    {
                        (await _database.Users.SingleAsync(user => user.MotigaId == player)).InQueue = false;
                    }

                    await _database.SaveChangesAsync();

                    _logger.LogInformation("Waiting for the server to become ready...");
                    await Task.Delay(TimeSpan.FromSeconds(15));

                    var ck2 = Convert.ToBase64String(Encoding.UTF8.GetBytes("imagoodcipherkey"));
                    var ck = Convert.ToBase64String(Encoding.UTF8.GetBytes("amotigadeveloper"));
                    var bcryptHmac = Convert.ToBase64String(Encoding.UTF8.GetBytes("totsagoodsuperlonghmacsecretkeys"));
                    var msg = new object[] { "match.ready", new
                    {
                        matchinfo = new
                        {
                            server = new
                            {
                                connstr = server,
                                map = map,
                            },
                            instanceid = "12",
                            token = ck + ck2 + bcryptHmac,
                            meta = new
                            {
                                moid = 2,
                            },
                        },
                    }};

                    foreach (var client in clients)
                    {
                        try
                        {
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
            _logger.LogInformation("{} is stopping.", nameof(MatchmakingService));

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
                        if (!IPAddress.TryParse(_configuration.MiceHost, out ip))
                        {
                            ip = (await Dns.GetHostAddressesAsync(_configuration.MiceHost))
                                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                                .FirstOrDefault();
                        }

                        return $"{ip}:{port}";
                    }
                }
                catch { }
            }
            return null;
        }
    }
}