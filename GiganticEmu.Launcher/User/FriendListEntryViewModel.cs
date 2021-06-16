using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher
{
    public class FriendListEntryViewModel : ReactiveObject
    {
        [Reactive]
        public FriendData Friend { get; set; } = default!;

        [Reactive]
        public bool IsLoading { get; set; } = false;

        public ReactiveCommand<Unit, Unit> RemoveFriend { get; }
        public ReactiveCommand<Unit, Unit> AcceptRequest { get; }
        public ReactiveCommand<Unit, Unit> DenyRequest { get; }

        public FriendListEntryViewModel()
        {
            RemoveFriend = ReactiveCommand.CreateFromTask(DoRemoveFriend);
            AcceptRequest = ReactiveCommand.CreateFromTask(DoAcceptRequest);
            DenyRequest = ReactiveCommand.CreateFromTask(DoDenyRequest);

            this.WhenAnyValue(x => x.Friend)
                .Do(x => IsLoading = false)
                .Subscribe();
        }

        private async Task DoRemoveFriend()
        {
            IsLoading = true;
            await Locator.Current.GetService<UserManager>()!.RemoveFriend(Friend.UserName);
        }

        private async Task DoAcceptRequest()
        {
            IsLoading = true;
            await Locator.Current.GetService<UserManager>()!.SendFriendRequest(Friend.UserName);
        }

        private async Task DoDenyRequest()
        {
            IsLoading = true;
            await Locator.Current.GetService<UserManager>()!.RemoveFriend(Friend.UserName);
        }
    }
}
