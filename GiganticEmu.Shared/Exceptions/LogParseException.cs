using System;

namespace GiganticEmu.Shared;

[Serializable]
public class LogParseException : Exception
{
    public LogParseException(string message, Exception? inner = null)
        : base(message, inner) { }

}
