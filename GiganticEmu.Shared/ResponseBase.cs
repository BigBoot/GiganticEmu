namespace GiganticEmu.Shared;

public record ResponseBase(RequestResult Code)
{
    public string Message { get; } = Code.GetDescription();
}
