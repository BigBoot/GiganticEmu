namespace GiganticEmu.Shared
{
    public class Player
    {
        public string Name { get; set; } = default!;
        public int MotigaId { get; set; }
        public string? MatchToken { get; set; } = null;
    }
}