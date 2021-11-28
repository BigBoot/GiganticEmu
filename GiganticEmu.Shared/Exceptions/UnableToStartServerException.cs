using System;

namespace GiganticEmu.Shared;

[Serializable]
public class UnableToStartServerException : Exception
{
    public string? _details { get; init; } = null;

    public UnableToStartServerException()
        : base(RequestResult.UnableToStartServer.GetDescription()) { }

    public UnableToStartServerException(Exception inner)
        : base(RequestResult.UnableToStartServer.GetDescription(), inner) { }

    public UnableToStartServerException(string details)
        : base(RequestResult.UnableToStartServer.GetDescription())
    {
        _details = details;
    }
}
