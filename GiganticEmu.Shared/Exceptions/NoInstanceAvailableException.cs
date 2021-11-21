using System;

namespace GiganticEmu.Shared
{
    [Serializable]
    public class NoInstanceAvailableException : Exception
    {
        public NoInstanceAvailableException() : base(RequestResult.NoInstanceAvailable.GetDescription()) { }

        public NoInstanceAvailableException(Exception inner)
            : base(RequestResult.NoInstanceAvailable.GetDescription(), inner) { }
    }
}