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

namespace GiganticEmu.Launcher
{
    public class FriendListViewModel : ReactiveObject
    {
        [Reactive]
        public bool IsLoading { get; set; } = false;

        [Reactive]
        public UserData? User { get; set; } = null;

        private SourceCache<FriendData, string> _friends = new SourceCache<FriendData, string>(x => x.UserName);
        public IObservableCollection<FriendListEntryViewModel> Friends { get; } = new ObservableCollectionExtended<FriendListEntryViewModel>();
        public ReactiveCommand<string, Unit> AddFriend { get; }

        public FriendListViewModel()
        {
            AddFriend = ReactiveCommand.CreateFromTask<string>(DoAddFriend);

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
                .Select(_ =>userManager.GetFriends().GetAwaiter().GetResult())
                .Select(result => result.Friends ?? new List<FriendData>())
                .Subscribe(friends =>
                {
                    foreach (var friend in friends) 
                    {
                        _friends.AddOrUpdate(friend);
                    }

                    _friends.RemoveKeys(_friends.Keys.Except(friends.Select(x => x.UserName)));
                });

            //_friends.Add(new FriendData()
            //{
            //    UserName = "TheLegend28",
            //    IconHash = "0418e2cea8ef594e6bc05ab3c3fe1fae0db09a2981d32b291992ddc2191075fe",
            //    IsOnline = true,
            //    CanAccept = false,
            //    HasAccepted = true,
            //    CanJoin = false
            //});

            //_friends.Add(new FriendData()
            //{
            //    UserName = "TheLegend29",
            //    IconHash = "e60c11bc0ea105260a7a345528e55b38d2ff4fef03124b8b295ee2c655faadf5",
            //    IsOnline = false,
            //    CanAccept = false,
            //    HasAccepted = true,
            //    CanJoin = false
            //});

            //_friends.Add(new FriendData()
            //{
            //    UserName = "TheLegend30",
            //    IconHash = "17887f87477a18d04aeb3012f23f23843606e0ab9a9b25664ac348274de89b0c",
            //    IsOnline = false,
            //    CanAccept = true,
            //    HasAccepted = true,
            //    CanJoin = false
            //});
        }

        private async Task DoAddFriend(string userName)
        {
            await Locator.Current.GetService<UserManager>()!.SendFriendRequest(userName);
        }
    }
}
