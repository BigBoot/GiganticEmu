using System.Collections.Generic;

namespace GiganticEmu.Shared;

public record MatchPostRequest
{
    public ICollection<string> Players { get; set; } = default!;
};
