using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class LoginViewModel : ReactiveObject
{
    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public UserData? User { get; set; } = null;

    [Reactive]
    public string UserName { get; set; } = "";

    [Reactive]
    public string Password { get; set; } = "";

    [Reactive]
    public ICollection<string> Errors { get; set; } = new List<string>();

    public ReactiveCommand<Unit, Unit> Login { get; }
    public ReactiveCommand<Unit, Unit> StartOffline { get; }

    public Interaction<DialogContent, Unit> OnError { get; } = new();

    public Interaction<DialogContent, bool> OnConfirm { get; } = new();

    public LoginViewModel()
    {
        Login = ReactiveCommand.CreateFromTask(DoLogin);
        StartOffline = ReactiveCommand.CreateFromTask(DoStartOffline);
    }

    private async Task DoLogin()
    {
        IsLoading = true;
        (User, Errors) = await Locator.Current.RequireService<UserManager>().Login(UserName, Password);
        if (User?.AuthToken is string token)
        {
            await Locator.Current.RequireService<CredentialStorage>().SaveToken(token);
        }
        IsLoading = false;
    }

    private async Task DoStartOffline()
    {
        var settings = Locator.Current.RequireService<Settings>();
        var launcher = Locator.Current.RequireService<GameLauncher>();
        launcher.StartGame(new GameLauncher.InteractionHandler
        {
            OnConfirm = async content => await OnConfirm.Handle(content),
            OnError = async content => await OnError.Handle(content),
        }, settings.OfflineName.Value, settings.OfflineName.Value);
    }
}
