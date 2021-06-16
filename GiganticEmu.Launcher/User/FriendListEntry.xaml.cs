using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Reactive.Linq;
using System.Reactive;

namespace GiganticEmu.Launcher
{
    public partial class FriendListEntry
    {
        public FriendListEntry()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.IsEnabled,
                    value => !value
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.UserName,
                    view => view.TextUserName.Text
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.IconHash,
                    view => view.IconUser.ImageSource,
                    value => new BitmapImage(new Uri($"https://seccdn.libravatar.org/avatar/{value}?d=pagan"))
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.IsOnline,
                    view => view.IconOnlineStatus.Foreground,
                    value => new SolidColorBrush(value ? Colors.Green : Colors.DarkGray)
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.IsOnline,
                    view => view.TextOnlineStatus.Text,
                    value => value ? "Online" : "Offline"
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.CanAccept,
                    view => view.ButtonFriendAccept.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.CanAccept,
                    view => view.ButtonFriendDeny.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.CanAccept,
                    view => view.ButtonFriendDelete.Visibility,
                    value => value ? Visibility.Collapsed : Visibility.Visible
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.CanJoin,
                    view => view.ButtonJoinGroup.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed
                )
                .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonFriendDelete, nameof(ButtonFriendAccept.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel!.RemoveFriend)
                    .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonFriendAccept, nameof(ButtonFriendAccept.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel!.AcceptRequest)
                    .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonFriendDeny, nameof(ButtonFriendAccept.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel!.DenyRequest)
                    .DisposeWith(disposables);
            });
        }
    }
}
