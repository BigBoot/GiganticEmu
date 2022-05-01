using System.Collections.Generic;

namespace GiganticEmu.Shared;

public record UserPWPostResponse(RequestResult Code) : ResponseBase(Code)
{
    public IEnumerable<UserPostError>? Errors { get; init; } = default;
}
