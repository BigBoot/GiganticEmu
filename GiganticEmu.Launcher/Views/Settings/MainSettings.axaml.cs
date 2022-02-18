using System.Reactive.Disposables;
using ReactiveUI;
using Avalonia.ReactiveUI;
using System;

namespace GiganticEmu.Launcher;

public partial class MainSettings : ReactiveUserControl<MainSettingsViewModel>
{
    public MainSettings()
    {
        InitializeComponent();
        ViewModel = new MainSettingsViewModel();

        LauncherBackground.Items = Enum.GetValues<Settings.BackgroundImage>();
        GameLanguage.Items = Enum.GetValues<Settings.Language>();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                    viewModel => viewModel.GameLanguage,
                    view => view.GameLanguage.SelectedItem
                )
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                    viewModel => viewModel.LauncherBackground,
                    view => view.LauncherBackground.SelectedItem
                )
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                    viewModel => viewModel.OfflineName,
                    view => view.OfflineName.Text
                )
                .DisposeWith(disposables);
        });
    }
}