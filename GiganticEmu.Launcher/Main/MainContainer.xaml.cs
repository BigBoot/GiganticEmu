using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace GiganticEmu.Launcher;

public partial class MainContainer
{
    public MainContainer()
    {
        ViewModel = new MainContainerViewModel();
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            Observable.FromEventPattern(ButtonStartGame, nameof(ButtonStartGame.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.StartGame)
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonStartServer, nameof(ButtonStartServer.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.StartServer)
                .DisposeWith(disposables);
        });
    }
}
