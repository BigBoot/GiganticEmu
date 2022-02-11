using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace GiganticEmu.Launcher;

public partial class MainSettings
{

    public MainSettings()
    {
        ViewModel = new MainSettingsViewModel();
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            GameLanguage.ItemsSource = Enum.GetValues<Settings.Language>();

            this.Bind(ViewModel,
                    viewModel => viewModel.GameLanguage,
                    view => view.GameLanguage.SelectedItem
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
