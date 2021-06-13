namespace GiganticEmu.Shared
{
    public record SessionGetResponse(RequestResult Code) : ResponseBase(Code)
    {
        public string? UserName { get; init; } = null!;
    }
}