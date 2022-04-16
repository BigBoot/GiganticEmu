using System.Reactive.Disposables;
using ReactiveUI;
using Avalonia.ReactiveUI;
using System;

namespace GiganticEmu.Launcher;

public partial class LinuxSettings : ReactiveUserControl<LinuxSettingsViewModel>
{
    public LinuxSettings()
    {
        InitializeComponent();
        ViewModel = new LinuxSettingsViewModel();

        CompatibilityTool.Items = Enum.GetValues<Settings.CompatiblityTool>();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                    viewModel => viewModel.CompatiblityTool,
                    view => view.CompatibilityTool.SelectedItem
                )
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                    viewModel => viewModel.MangoHudEnabled,
                    view => view.MangoHud.IsChecked
                )
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                    viewModel => viewModel.GameModeEnabled,
                    view => view.GameMode.IsChecked
                )
                .DisposeWith(disposables);
        });
    }
}