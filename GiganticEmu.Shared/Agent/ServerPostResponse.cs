namespace GiganticEmu.Shared
{
    public record ServerPostResponse(RequestResult Code) : ResponseBase(Code)
    {
        public int Port { get; init; } = default!;
    }
}