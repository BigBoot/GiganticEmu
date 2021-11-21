using System;

namespace GiganticEmu.Shared
{
    [Serializable]
    public class InvalidInstanceException : Exception
    {
        public int? _instanceId { get; init; } = null;

        public InvalidInstanceException()
            : base(RequestResult.InvalidInstance.GetDescription()) { }

        public InvalidInstanceException(Exception inner)
            : base(RequestResult.InvalidInstance.GetDescription(), inner) { }

        public InvalidInstanceException(int instanceId)
            : base(RequestResult.InvalidInstance.GetDescription())
        {
            _instanceId = instanceId;
        }
    }
}