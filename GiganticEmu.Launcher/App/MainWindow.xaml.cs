using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace GiganticEmu.Launcher
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.ContainerContent.Visibility,
                    value => value ? Visibility.Hidden : Visibility.Visible
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.IsLoading,
                    view => view.ProgressLoading.Visibility,
                    value => value ? Visibility.Visible : Visibility.Hidden
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.User,
                    view => view.PageMain.Visibility,
                    value => value == null ? Visibility.Hidden : Visibility.Visible
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.User,
                    view => view.PageLogin.Visibility,
                    value => value == null ? Visibility.Visible : Visibility.Hidden
                )
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.User,
                    view => view.FriendListColumn.Width,
                    value => value == null ? new GridLength(0) : new GridLength(1, GridUnitType.Star)
                )
                .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.User,
                    view => view.PageLogin.ViewModel!.User
                )
                .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.User,
                    view => view.PageMain.ViewModel!.User
                )
                .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.User,
                    view => view.FriendList.ViewModel!.User
                )
                .DisposeWith(disposables);


                //new ToastContentBuilder()
                //    .AddText("TheLegend27 has invited you to a group.")
                //    .AddArgument("action", "open")
                //    .AddButton(new ToastButton()
                //        .SetContent("Accept")
                //        .AddArgument("action", "accept"))
                //    .AddButton(new ToastButton()
                //        .SetContent("Deny")
                //        .AddArgument("action", "accept"))
                //    .Show(toast =>
                //    {
                //        toast.Tag = "<session_invite_id>";
                //        toast.Group = "session_invite";
                //    });

                //new ToastContentBuilder()
                //    .AddText("TheLegend27 want's to add you as a friend.")
                //    .AddArgument("action", "open")
                //    .AddButton(new ToastButton()
                //        .SetContent("Accept")
                //        .AddArgument("action", "accept"))
                //    .AddButton(new ToastButton()
                //        .SetContent("Deny")
                //        .AddArgument("action", "accept"))
                //    .Show(toast =>
                //    {
                //        toast.Tag = "<friend_invite_id>";
                //        toast.Group = "friend_invite";
                //    });

                //Observable.FromEvent<OnActivated, ToastNotificationActivatedEventArgsCompat>(
                //    handler => { OnActivated onActivated = (args) => handler(args); return onActivated; },
                //    handler => ToastNotificationManagerCompat.OnActivated += handler,
                //    handler => ToastNotificationManagerCompat.OnActivated -= handler
                //)
                //.Select(ev => new { args = ToastArguments.Parse(ev.Argument), input = ev.UserInput })
                //.Do(ev =>
                //{
                //    Application.Current.Dispatcher.Invoke(delegate
                //    {
                //        MessageBox.Show("Toast activated. action: " + ev.args["action"]);
                //    });
                //})
                //.Subscribe()
                //.DisposeWith(disposables);


                ViewModel.RestoreUser.Execute();
            });
        }
    }
}
