namespace GiganticEmu.Shared;

public record DiscordPostTokenResponse(RequestResult Code) : ResponseBase(Code)
{
    public string? Token { get; set; }
}
