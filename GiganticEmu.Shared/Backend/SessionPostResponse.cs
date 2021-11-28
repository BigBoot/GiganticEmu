using System;

namespace GiganticEmu.Shared;

public record SessionPostResponse(RequestResult Code) : ResponseBase(Code)
{
    public string? AuthToken { get; init; } = default!;
    public DateTimeOffset? Expiry = default!;
}
