using System.Collections.Generic;

namespace GiganticEmu.Shared;

public record MatchTokenPostRequest
{
    public string DiscordId { get; set; } = default!;
};
