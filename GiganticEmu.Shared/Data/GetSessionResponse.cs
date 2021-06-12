using System;

namespace GiganticEmu.Shared
{
    public record SessionGetResponse(RequestResult Code, string Message, string? Username = null);
}