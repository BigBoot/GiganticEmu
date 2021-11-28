using System.Collections.Generic;

namespace GiganticEmu.Shared;

public record UserPostResponse(RequestResult Code) : ResponseBase(Code)
{
    public IEnumerable<UserPostError>? Errors { get; init; } = default;

    public string? AuthToken { get; init; } = default;
}
