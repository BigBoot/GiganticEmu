using System.Collections.Generic;

namespace GiganticEmu.Shared
{
    public record UserPostResponse(RequestResult Code, string Message, IEnumerable<UserPostError>? Errors = default, string? AuthToken = null);
}