using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Avalonia.Visuals.Media.Imaging;
using ReactiveUI;
using Splat;

namespace GiganticEmu.Launcher
{
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel();

            this.WhenActivated(disposables =>
            {
                var settings = Locator.Current.RequireService<Settings>();

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.IsLoading,
                        view => view.ContainerContent.IsVisible,
                        value => !value
                    )
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.IsLoading,
                        view => view.ProgressLoading.IsVisible
                    )
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CurrentPage,
                        view => view.PageMain.IsVisible,
                        value => value == AppViewModel.Page.Main
                    )
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CurrentPage,
                        view => view.FriendListContainer.ColumnDefinitions[1].Width,
                        value => value == AppViewModel.Page.Main
                            ? new GridLength(1, GridUnitType.Star)
                            : new GridLength(0)
                    )
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CurrentPage,
                        view => view.PageLogin.IsVisible,
                        value => value == AppViewModel.Page.Login
                    )
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CurrentPage,
                        view => view.PageSettings.IsVisible,
                        value => value == AppViewModel.Page.Settings
                    )
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CurrentPage,
                        view => view.PageSetupGame.IsVisible,
                        value => value == AppViewModel.Page.SetupGame
                    )
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CurrentPage,
                        view => view.ButtonSettings.IsVisible,
                        value => value is AppViewModel.Page.Login or AppViewModel.Page.Main
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

                settings.Background
                    .Select(bg => new Bitmap(Locator.Current.RequireService<IAssetLoader>().Open($"/Resources/background_{bg.ToString().ToLowerInvariant()}.jpg")))
                    .Select(bm => new ImageBrush(bm)
                    {
                        Stretch = Stretch.UniformToFill,
                        BitmapInterpolationMode = BitmapInterpolationMode.HighQuality
                    })
                    .BindTo(this, x => x.Background)
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

                this.WhenAnyValue(x => x.DialogContent.Children.Count)
                    .Select(value => value > 0)
                    .BindTo(DialogHost, view => view.IsOpen)
                    .DisposeWith(disposables);

                ViewModel.OnUpdateAvailable.RegisterHandler(async interaction =>
                {
                    var result = await this.ShowUpdateDialog(async dialog =>
                    {
                        dialog.Changelog = interaction.Input.Changelog;
                    });

                    interaction.SetOutput(result);
                })
                .DisposeWith(disposables);

                ViewModel.OnUpdate.RegisterHandler(async interaction =>
                {
                    var result = await this.ShowProgressDialog(async dialog =>
                    {
                        dialog.Title = "Downloading Update";
                        interaction.Input
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Finally(() => dialog.OnFinished.Handle(Unit.Default).Wait())
                            .Subscribe((progress) => dialog.Value = progress);
                    });

                    interaction.SetOutput(result);
                })
                .DisposeWith(disposables);

                ViewModel.OnUpdateFinishing.RegisterHandler(async interaction =>
                {
                    var result = await this.ShowProgressDialog(async dialog =>
                    {
                        dialog.Title = "Performing Update";
                        dialog.IsIndeterminate = true;
                    });

                    interaction.SetOutput(Unit.Default);
                })
                .DisposeWith(disposables);

                ViewModel.RestoreUser.Execute();
                ViewModel.CheckForUpdate.Execute();
            });
        }
    }
}