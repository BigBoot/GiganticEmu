using System.Collections.Generic;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiganticEmu.Agent.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AgentConfiguration _configuration;
        private readonly ServerManager _serverManager;

        public string Title { get => _configuration.Title; }
        public int MaxInstances { get => _configuration.MaxInstances; }
        public int RunningInstances { get => _serverManager.RunningInstances; }
        public int AvailableInstances { get => MaxInstances - RunningInstances; }
        public ICollection<Map> Maps { get => Map.ALL_MAPS.Values; }
        public ICollection<Creature> Creatures { get => Creature.ALL_CREATURES.Values; }
        public string[] DefaultCreatures { get => _configuration.DefaultCreatures; }
        public string? Instance = null;

        public IndexModel(ILogger<IndexModel> logger, IOptions<AgentConfiguration> configuration, ServerManager serverManager)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _serverManager = serverManager;
        }

        public async Task OnPostAsync(string map, int maxPlayers, string creature0, string creature1, string creature2)
        {
            var port = await _serverManager.StartInstance(map, maxPlayers, (creature0, creature1, creature2));
            Instance = $"{_configuration.ServerHost}:{port}";
        }
    }
}