using System.Collections.Generic;

namespace GiganticEmu.Shared;

public record MatchTokenPostResponse(RequestResult Code) : ResponseBase(Code)
{
    public string Name { get; set; } = default!;
    public string MatchToken { get; set; } = default!;
}
