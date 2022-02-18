using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace GiganticEmu.Launcher;

public partial class MainContainer : ReactiveUserControl<MainContainerViewModel>
{
    public MainContainer()
    {
        InitializeComponent();
        ViewModel = new MainContainerViewModel();

        this.WhenActivated(disposables =>
        {
            Observable.FromEventPattern(ButtonStartGame, nameof(ButtonStartGame.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.StartGame)
                .DisposeWith(disposables);
        });
    }
}