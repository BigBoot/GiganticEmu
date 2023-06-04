using System.Collections.Generic;

namespace GiganticEmu.Shared;

public record MatchPostResponse(RequestResult Code) : ResponseBase(Code)
{
    public record Team(ICollection<string> Players, double AverageSkill);

    public Team Team1 { get; set; } = default!;
    public Team Team2 { get; set; } = default!;

    public string ReportToken { get; set; } = default!;
}
