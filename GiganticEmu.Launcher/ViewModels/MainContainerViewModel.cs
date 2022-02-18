using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Reactive;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher;

public class MainContainerViewModel : ReactiveObject
{
    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public UserData? User { get; set; } = null;

    public ReactiveCommand<Unit, Unit> StartGame { get; }

    public MainContainerViewModel()
    {
        StartGame = ReactiveCommand.CreateFromTask(DoStartGame);
    }

    private async Task DoStartGame()
    {
        var launcher = Locator.Current.RequireService<GameLauncher>();
        launcher.StartGame(User?.UserName, User?.UserName, User?.AuthToken);
    }
}
