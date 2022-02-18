using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
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

    public ReactiveCommand<Unit, Unit> RemoveFriend { get; }
    public ReactiveCommand<Unit, Unit> AcceptRequest { get; }
    public ReactiveCommand<Unit, Unit> DenyRequest { get; }
    public ReactiveCommand<Unit, Unit> InviteFriend { get; }
    public ReactiveCommand<Unit, Unit> AcceptInvite { get; }
    public ReactiveCommand<Unit, Unit> DenyInvite { get; }

    public Interaction<(string, IEnumerable<string>), Unit> Error { get; } = new();

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

    private async Task DoRemoveFriend()
    {
        IsLoading = true;
        if (await Locator.Current.RequireService<UserManager>().RemoveFriend(Friend.UserName) is { Errors.Count: > 1 } result)
        {
            await Error.Handle(("Unable to remove friend.", result.Errors));
        };
    }

    private async Task DoAcceptRequest()
    {
        IsLoading = true;
        if (await Locator.Current.RequireService<UserManager>().SendFriendRequest(Friend.UserName) is { Errors.Count: > 1 } result)
        {
            await Error.Handle(("Unable to accept the request.", result.Errors));
        }
    }

    private async Task DoDenyRequest()
    {
        IsLoading = true;
        if (await Locator.Current.RequireService<UserManager>().RemoveFriend(Friend.UserName) is { Errors.Count: > 1 } result)
        {
            await Error.Handle(("Unable to deny the request friend.", result.Errors));
        }
    }

    private async Task DoInviteFriend()
    {
        IsLoading = true;
        if (await Locator.Current.RequireService<UserManager>().InviteToGroup(Friend.UserName) is { Errors.Count: > 1 } result)
        {
            await Error.Handle(("Unable to send invite.", result.Errors));
        }
    }

    private async Task DoAcceptInvite()
    {
        IsLoading = true;
        if (await Locator.Current.RequireService<UserManager>().InviteToGroup(Friend.UserName) is { Errors.Count: > 1 } result)
        {
            await Error.Handle(("Unable to accept invite.", result.Errors));
        }
    }

    private async Task DoDenyInvite()
    {
        IsLoading = true;
        if (await Locator.Current.RequireService<UserManager>().RemoveFriend(Friend.UserName) is { Errors.Count: > 1 } result)
        {
            await Error.Handle(("Unable to deny invite.", result.Errors));
        }
    }
}
