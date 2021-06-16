using IPrompt;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace GiganticEmu.Launcher
{
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
                    value => value?.UserName ?? ""
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Friends,
                    view => view.ListFriends.ItemsSource
                )
                .DisposeWith(disposables);

                Observable.FromEventPattern(ButtonAddFriend, nameof(ButtonAddFriend.Click))
                    .Select(_ => IInputBox.Show("Please enter the username to add", "Add friend", System.Windows.MessageBoxImage.None))
                    .InvokeCommand(this, x => x.ViewModel!.AddFriend)
                    .DisposeWith(disposables);
            });
        }
    }
}
