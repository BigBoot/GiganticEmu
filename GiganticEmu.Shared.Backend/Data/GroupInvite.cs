using System;

namespace GiganticEmu.Shared.Backend
{
    public class GroupInvite : EntityBase
    {
        public Guid UserId { get; set; } = default!;

        public Guid InvitedUserId { get; set; } = default!;

        public User User { get; set; } = default!;

        public User InvitedUser { get; set; } = default!;
    }
}