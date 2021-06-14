using System.Collections.Generic;

namespace GiganticEmu.Shared
{
    public record QueueGetResponse(RequestResult Code) : ResponseBase(Code)
    {
        public ICollection<string> Players { get; init; } = default!;
    }
}