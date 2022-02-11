using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace GiganticEmu.Launcher;

public partial class FriendList
{
    public FriendList()
    {
        ViewModel = new FriendListViewModel();
        InitializeComponent();

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
                view => view.ListFriends.ItemsSource
            )
            .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel!.User)
                .Where(x => x == null)
                .Subscribe(x => this.PopupUser.IsOpen = false)
                .DisposeWith(disposables);


            Observable.FromEventPattern(ButtonLogout, nameof(ButtonLogout.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.Logout)
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonLogout, nameof(ButtonLogout.Click))
                .Subscribe(x => this.PopupUser.IsOpen = false)
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonAddFriend, nameof(ButtonAddFriend.Click))
                .Select(_ => Prompt.ShowInputBox("Please enter the username to add", "Add friend", System.Windows.MessageBoxImage.None, owner: App.Current.MainWindow))
                .Where(x => x != null)
                .InvokeCommand(this, x => x.ViewModel!.AddFriend!)
                .DisposeWith(disposables);
        });
    }
}
