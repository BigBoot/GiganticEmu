namespace GiganticEmu.Shared
{
    public record SessionPostRequest()
    {
        public string UserName { get; init; } = default!;
        public string Password { get; init; } = default!;
        public string RememberMe { get; init; } = default!;
    }
}