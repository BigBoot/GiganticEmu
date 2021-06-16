namespace GiganticEmu.Launcher
{
    public record UserData
    {
        public string UserName { get; init; } = default!;

        public string AuthToken { get; init; } = default!;
    }
}
