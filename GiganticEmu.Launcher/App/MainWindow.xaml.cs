using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GiganticEmu.Launcher;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new AppViewModel();

        var background = new Random().Next(1, 4);
        WindowBackgroundBrush.ImageSource = new BitmapImage(new Uri($"/Resources/background_{background}.jpg", UriKind.Relative));
        WindowBackgroundImage.Source = WindowBackgroundBrush.ImageSource;

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
                viewModel => viewModel.CurrentPage,
                view => view.PageMain.Visibility,
                value => value == AppViewModel.Page.Main ? Visibility.Visible : Visibility.Hidden
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentPage,
                view => view.FriendListColumn.Width,
                value => value == AppViewModel.Page.Main ?  new GridLength(1, GridUnitType.Star) : new GridLength(0)
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentPage,
                view => view.PageLogin.Visibility,
                value => value == AppViewModel.Page.Login ? Visibility.Visible : Visibility.Hidden
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentPage,
                view => view.PageSettings.Visibility,
                value => value == AppViewModel.Page.Settings ? Visibility.Visible : Visibility.Hidden
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentPage,
                view => view.PageSetupGame.Visibility,
                value => value == AppViewModel.Page.SetupGame ? Visibility.Visible : Visibility.Hidden
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentPage,
                view => view.ButtonSettings.Visibility,
                value => value is AppViewModel.Page.Login or AppViewModel.Page.Main ? Visibility.Visible : Visibility.Hidden
            )
            .DisposeWith(disposables);

            this.Bind(ViewModel,
                viewModel => viewModel.SettingsVisible,
                view => view.PageSettings.ViewModel!.SettingsVisible
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

            this.Bind(ViewModel,
                viewModel => viewModel.PageTitle,
                view => view.PageTitle.Text
            ).DisposeWith(disposables);

            Observable.FromEventPattern(ButtonSettings, nameof(ButtonSettings.Click))
                .Select(x => Unit.Default)
                .Subscribe(x => ViewModel.SettingsVisible = true)
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
