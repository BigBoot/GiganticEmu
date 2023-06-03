namespace GiganticEmu.Shared;

public record DiscordGetUserResponse(RequestResult Code) : ResponseBase(Code)
{
    public string? Name { get; set; }
}
