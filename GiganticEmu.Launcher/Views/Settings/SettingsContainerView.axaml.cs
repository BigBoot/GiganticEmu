using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using GiganticEmu.Shared;

namespace GiganticEmu.Launcher;

public partial class SettingsContainerView : ReactiveUserControl<SettingsContainerViewModel>
{
    public SettingsContainerView()
    {
        InitializeComponent();
        ViewModel = new SettingsContainerViewModel();

        this.WhenActivated(disposables =>
        {
            Observable.FromEventPattern(ButtonCancel, nameof(ButtonCancel.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.Cancel)
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonSave, nameof(ButtonSave.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.SaveSettings)
                .DisposeWith(disposables);

            PageLinuxSettings.Parent!.IsVisible = PlatformUtils.IsLinux;
            PageRushLoadoutSettings.Parent!.IsVisible = GameLauncher.GiganticBuild >= GameUtils.BUILD_THROWBACK_EVENT;

            ViewModel.Pages = new SettingsContainerViewModel.ISettingsPageViewModel[]
            {
                PageMainSettings.ViewModel!,
                PageLinuxSettings.ViewModel!,
                PageRushLoadoutSettings.ViewModel!,
            }.ToList();
        });
    }
}