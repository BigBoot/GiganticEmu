using System.Collections.Generic;

namespace GiganticEmu.Shared
{
    public class MatchResult
    {
        public enum Team { Team1, Team2 }

        public ICollection<Player> Team1 { get; set; } = default!;
        public ICollection<Player> Team2 { get; set; } = default!;
        public Team? Winner { get; set; } = null;
    }
}