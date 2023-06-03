using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GiganticEmu.Launcher;

public partial class FriendList : ReactiveUserControl<FriendListViewModel>
{
    public void OnLogoutClicked(object sender, RoutedEventArgs args)
    {
        ViewModel?.Logout?.Execute().Wait();
        PopupUser.IsOpen = false;
    }
    public void OnLinkDiscordClicked(object sender, RoutedEventArgs args)
    {
        ViewModel?.LinkDiscord?.Execute();
        PopupUser.IsOpen = false;
    }

    public FriendList()
    {
        InitializeComponent();
        ViewModel = new FriendListViewModel();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel,
                viewModel => viewModel.User,
                view => view.TextUserName.Text,
                value => (value?.UserName ?? "") + " ▼"
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Friends,
                view => view.ListFriends.Items
            )
            .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel!.User)
                .Where(x => x == null)
                .Subscribe(x => PopupUser.IsOpen = false)
                .DisposeWith(disposables);


            Observable.FromEventPattern(ButtonUserName, nameof(Button.Click))
                .Subscribe(_ => PopupUser.IsOpen = true)
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonAddFriend, nameof(ButtonAddFriend.Click))
                .SelectMany(_ => Observable.FromAsync(async () => await this.ShowInputDialog(async dialog =>
                {
                    dialog.Title = "Please enter the username to add";
                    dialog.CanCancel = true;
                    dialog.Icon = "HelpCircleOutline";
                })))
                .Where(x => x != null && x != "")
                .InvokeCommand(this, x => x.ViewModel!.AddFriend!)
                .DisposeWith(disposables);
        });
    }
}