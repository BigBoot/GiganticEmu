using GiganticEmu.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class FriendData : ReactiveObject
{

    [Reactive]
    public string UserName { get; set; } = default!;

    [Reactive]
    public string IconHash { get; set; } = default!;

    [Reactive]
    public UserStatus Status { get; set; } = UserStatus.Unknown;

    [Reactive]
    public bool CanAccept { get; set; } = false;

    [Reactive]
    public bool CanInvite { get; set; } = false;

    [Reactive]
    public bool CanJoin { get; set; } = false;

    [Reactive]
    public bool HasAccepted { get; set; } = false;
}
