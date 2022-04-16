using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher;

public class MainContainerViewModel : ReactiveObject
{
    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public UserData? User { get; set; } = null;

    public ReactiveCommand<Unit, Unit> StartGame { get; }

    public Interaction<DialogContent, Unit> OnError { get; } = new();

    public Interaction<DialogContent, bool> OnConfirm { get; } = new();

    public MainContainerViewModel()
    {
        StartGame = ReactiveCommand.CreateFromTask(DoStartGame);
    }

    private async Task DoStartGame()
    {
        var launcher = Locator.Current.RequireService<GameLauncher>();
        launcher.StartGame(new GameLauncher.InteractionHandler
        {
            OnConfirm = async content => await OnConfirm.Handle(content),
            OnError = async content => await OnError.Handle(content),
        }, User?.UserName, User?.UserName, User?.AuthToken);
    }
}
