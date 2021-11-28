using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher;

public class FriendListEntryViewModel : ReactiveObject
{
    [Reactive]
    public FriendData Friend { get; set; } = default!;

    [Reactive]
    public bool IsLoading { get; set; } = false;

    public ReactiveCommand<Unit, UserManager.GeneralResult> RemoveFriend { get; }
    public ReactiveCommand<Unit, UserManager.GeneralResult> AcceptRequest { get; }
    public ReactiveCommand<Unit, UserManager.GeneralResult> DenyRequest { get; }
    public ReactiveCommand<Unit, UserManager.GeneralResult> InviteFriend { get; }
    public ReactiveCommand<Unit, UserManager.GeneralResult> AcceptInvite { get; }
    public ReactiveCommand<Unit, UserManager.GeneralResult> DenyInvite { get; }

    public FriendListEntryViewModel()
    {
        RemoveFriend = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(DoRemoveFriend));
        AcceptRequest = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(DoAcceptRequest));
        DenyRequest = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(DoDenyRequest));
        InviteFriend = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(DoInviteFriend));
        AcceptInvite = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(DoAcceptInvite));
        DenyInvite = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(DoDenyInvite));

        this.WhenAnyValue(x => x.Friend)
            .Do(x => IsLoading = false)
            .Subscribe();
    }

    private async Task<UserManager.GeneralResult> DoRemoveFriend()
    {
        IsLoading = true;
        return await Locator.Current.GetService<UserManager>()!.RemoveFriend(Friend.UserName);
    }

    private async Task<UserManager.GeneralResult> DoAcceptRequest()
    {
        IsLoading = true;
        return await Locator.Current.GetService<UserManager>()!.SendFriendRequest(Friend.UserName);
    }

    private async Task<UserManager.GeneralResult> DoDenyRequest()
    {
        IsLoading = true;
        return await Locator.Current.GetService<UserManager>()!.RemoveFriend(Friend.UserName);
    }

    private async Task<UserManager.GeneralResult> DoInviteFriend()
    {
        IsLoading = true;
        return await Locator.Current.GetService<UserManager>()!.InviteToGroup(Friend.UserName);
    }

    private async Task<UserManager.GeneralResult> DoAcceptInvite()
    {
        IsLoading = true;
        return await Locator.Current.GetService<UserManager>()!.InviteToGroup(Friend.UserName);
    }

    private async Task<UserManager.GeneralResult> DoDenyInvite()
    {
        IsLoading = true;
        // return await Locator.Current.GetService<UserManager>()!.RemoveFriend(Friend.UserName);
        throw new NotImplementedException();
    }
}
