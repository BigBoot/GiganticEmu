using System.Collections.Generic;

namespace GiganticEmu.Shared;

public record MatchPostResponse(RequestResult Code) : ResponseBase(Code)
{
    public record Player(string DiscordId, string? Name, string? MatchToken);
    public record Team(ICollection<Player> Players, double AverageSkill);

    public Team Team1 { get; set; } = default!;
    public Team Team2 { get; set; } = default!;

    public string ReportToken { get; set; } = default!;
}
