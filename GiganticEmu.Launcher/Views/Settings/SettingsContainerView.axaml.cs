using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

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

            ViewModel.Pages = new[] { PageMainSettings }
                .Select(page => page.ViewModel)
                .OfType<SettingsContainerViewModel.SettingsPageViewModel>()
                .ToList();
        });
    }
}