using System.Collections.Generic;

namespace GiganticEmu.Shared
{
    public record FriendsGetResponse(RequestResult Code) : ResponseBase(Code)
    {
        public record FriendData(string UserName, bool Accepted, bool IsOnline, string IconHash);

        public record FriendRequestData(string UserName, string IconHash);

        public ICollection<FriendData> Friends { get; init; } = new List<FriendData>();

        public ICollection<FriendRequestData> FriendRequests { get; init; } = new List<FriendRequestData>();
    }
}