namespace GiganticEmu.Shared
{
    public record UserPostRequest()
    {
        public string Email { get; init; } = null!;
        public string UserName { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}