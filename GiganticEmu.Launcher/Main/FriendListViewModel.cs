using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher;

public class FriendListViewModel : ReactiveObject
{
    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public UserData? User { get; set; } = null;

    private SourceCache<FriendData, string> _friends = new SourceCache<FriendData, string>(x => x.UserName);
    public IObservableCollection<FriendListEntryViewModel> Friends { get; } = new ObservableCollectionExtended<FriendListEntryViewModel>();
    public ReactiveCommand<string, Unit> AddFriend { get; }
    public ReactiveCommand<Unit, Unit> Logout { get; }

    public FriendListViewModel()
    {
        AddFriend = ReactiveCommand.CreateFromTask<string>(DoAddFriend);
        Logout = ReactiveCommand.CreateFromTask(DoLogout);

        _friends.Connect()
            .AutoRefresh()
            .Sort(Comparer<FriendData>.Create((a, b) => (a.CanAccept, b.CanAccept) switch
            {
                (true, false) => -1,
                (false, true) => +1,
                _ => a.UserName.CompareTo(b.UserName)
            }))
            .Transform(x => new FriendListEntryViewModel() { Friend = x })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(Friends)
            .Subscribe();


        var userManager = Locator.Current.GetService<UserManager>()!;

        Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(5))
            .Select(_ => userManager.GetFriends().GetAwaiter().GetResult())
            .Select(result => result.Friends ?? new List<FriendData>())
            .Subscribe(friends =>
            {
                foreach (var friend in friends)
                {
                    _friends.AddOrUpdate(friend);
                }

                _friends.RemoveKeys(_friends.Keys.Except(friends.Select(x => x.UserName)));
            });
    }

    private async Task DoAddFriend(string userName)
    {
        await Locator.Current.GetService<UserManager>()!.SendFriendRequest(userName);
    }

    private async Task DoLogout()
    {
        IsLoading = true;
        await Locator.Current.GetService<CredentialStorage>()!.ClearToken();
        User = null;
        IsLoading = false;
    }
}
