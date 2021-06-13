namespace GiganticEmu.Shared
{
    public record VersionGetResponse(RequestResult Code) : ResponseBase(Code)
    {
        public int ApiVersion { get; init; } = default!;
        public int AgentVersionMajor { get; init; } = default!;
        public int AgentVersionMinor { get; init; } = default!;
        public int AgentVersionPatch { get; init; } = default!;
    }
}