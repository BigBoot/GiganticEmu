using System;

namespace GiganticEmu.Shared.Backend
{
    public class Friend : EntityBase
    {
        public Guid UserId { get; set; } = default!;

        public Guid FriendUserId { get; set; } = default!;

        public bool Accepted { get; set; } = false!;

        public User User { get; set; } = default!;

        public User FriendUser { get; set; } = default!;
    }
}