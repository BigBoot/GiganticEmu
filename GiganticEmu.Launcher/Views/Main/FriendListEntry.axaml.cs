using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using GiganticEmu.Shared;
using Polly;
using ReactiveUI;

namespace GiganticEmu.Launcher;

public partial class FriendListEntry : ReactiveUserControl<FriendListEntryViewModel>
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

            this.WhenAnyValue(x => x.ViewModel!.Friend.IconHash)
                .SelectMany(hash => Observable.FromAsync(async () =>
                    await $"https://seccdn.libravatar.org/avatar/{hash}?d=pagan"
                        .WithPolly(policy => policy.RetryAsync(3))
                        .Cached()
                        .GetBytesAsync()
                ))
                .Select(data => new Bitmap(new MemoryStream(data)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(bmp => new ImageBrush(bmp) { Stretch = Stretch.UniformToFill })
                .BindTo(this, view => view.IconUser.Fill)
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
                view => view.ButtonFriendAccept.IsVisible
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Friend.CanAccept,
                view => view.ButtonFriendDeny.IsVisible
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Friend.CanAccept,
                view => view.ButtonFriendDelete.IsVisible,
                value => !value
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Friend.CanJoin,
                view => view.ButtonJoinGroup.IsVisible
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Friend.CanInvite,
                view => view.ButtonInviteGroup.IsVisible
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


            ViewModel!.Error.RegisterHandler(async interaction =>
           {
               await this.ShowAlertDialog(async dialog =>
               {
                   var (context, errors) = interaction.Input;

                   dialog.Title = context;
                   dialog.Text = string.Join('\n', errors);
                   dialog.Icon = "AlertCircleOutline";

                   dialog.Buttons.Edit(buttons =>
                   {
                       buttons.Add(new AlertDialogViewModel.Button("Ok", "ok"));
                   });
               });

               interaction.SetOutput(Unit.Default);
           });

            Observable.FromEventPattern(ButtonJoinGroup, nameof(ButtonJoinGroup.Click))
                .InvokeCommand(this, x => x.ViewModel!.AcceptInvite)
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonInviteGroup, nameof(ButtonJoinGroup.Click))
                .InvokeCommand(this, x => x.ViewModel!.InviteFriend)
                .DisposeWith(disposables);
        });
    }
}