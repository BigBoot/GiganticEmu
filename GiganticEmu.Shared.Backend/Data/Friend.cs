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

        public UserStatus Status
        {
            get => true switch
            {
                _ when Accepted => true switch
                {
                    _ when FriendUser.LastOnline.AddMinutes(5) >= DateTimeOffset.UtcNow => true switch
                    {
                        _ when FriendUser.InQueue => UserStatus.InQueue,
                        _ when FriendUser.SessionId != null => UserStatus.InGame,
                        _ => UserStatus.Online,
                    },
                    _ => UserStatus.Offline
                },
                _ => UserStatus.Unknown
            };
        }
    }
}