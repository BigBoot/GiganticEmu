using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Reactive.Linq;
using System.Reactive;
using GiganticEmu.Shared;

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
                    viewModel => viewModel.Friend.Status,
                    view => view.IconOnlineStatus.Foreground,
                    value => new SolidColorBrush(value switch
                    {
                        UserStatus.InGame => Colors.Green,
                        UserStatus.Online => Colors.Green,
                        _ => Colors.DarkGray
                    })
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.Status,
                    view => view.TextOnlineStatus.Text,
                    value => value switch
                    {
                        UserStatus.InMatch => "In a match",
                        UserStatus.InQueue => "In matchmaking",
                        UserStatus.InGame => "In Game",
                        UserStatus.Online => "Online",
                        UserStatus.Offline => "Offline",
                        UserStatus.Unknown => "Unknown"
                    }
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

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friend.CanInvite,
                    view => view.ButtonInviteGroup.Visibility,
                    value => value ? Visibility.Visible : Visibility.Collapsed
                )
                .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonFriendDelete, nameof(ButtonFriendDelete.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel!.RemoveFriend)
                    .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonFriendAccept, nameof(ButtonFriendAccept.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel!.AcceptRequest)
                    .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonFriendDeny, nameof(ButtonFriendDeny.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel!.DenyRequest)
                    .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonJoinGroup, nameof(ButtonJoinGroup.Click))
                    .Select(_ => ViewModel!.AcceptInvite.Execute())
                    .Concat()
                    .SubscribeOn(RxApp.MainThreadScheduler)
                    .Do(result =>
                    {
                        if (result.Errors.Count > 0)
                        {
                            MessageBox.Show(string.Join('\n', result.Errors), "Unable to accept invite.", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    })
                    .Subscribe()
                    .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonInviteGroup, nameof(ButtonInviteGroup.Click))
                    .Select(_ => ViewModel!.InviteFriend.Execute())
                    .Concat()
                    .SubscribeOn(RxApp.MainThreadScheduler)
                    .Do(result =>
                    {
                        if(result.Errors.Count > 0)
                        {
                            MessageBox.Show(string.Join('\n', result.Errors), "Unable to send invite.", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    })
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }
    }
}
