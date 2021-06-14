namespace GiganticEmu.Shared
{
    public record UserPostRequest()
    {
        public string Email { get; init; } = default!;
        public string UserName { get; init; } = default!;
        public string Password { get; init; } = default!;
    }
}