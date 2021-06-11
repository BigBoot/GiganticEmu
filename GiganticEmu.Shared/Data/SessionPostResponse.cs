using System;

namespace GiganticEmu.Shared
{
    public record SessionPostResponse(RequestResult Code, string Message, string? AuthToken = null, DateTimeOffset? Expiry = null);
}