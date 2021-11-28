using System.Collections.Generic;
using System.Reactive;
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

    public LoginViewModel()
    {
        Login = ReactiveCommand.CreateFromTask(DoLogin);
    }

    private async Task DoLogin()
    {
        IsLoading = true;
        (User, Errors) = await Locator.Current.GetService<UserManager>()!.Login(UserName, Password);
        if (User?.AuthToken is string token)
        {
            await Locator.Current.GetService<CredentialStorage>()!.SaveToken(token);
        }
        IsLoading = false;
    }
}
