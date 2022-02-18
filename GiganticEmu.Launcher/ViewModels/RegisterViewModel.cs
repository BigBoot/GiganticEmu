using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher;

public class RegisterViewModel : ReactiveObject
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
    public string PasswordConfirm { get; set; } = "";

    [Reactive]
    public string Email { get; set; } = "";

    [Reactive]
    public ICollection<string> Errors { get; set; } = new List<string>();

    public ReactiveCommand<Unit, Unit> Register { get; }

    public RegisterViewModel()
    {
        Register = ReactiveCommand.CreateFromTask(DoRegister);
    }

    private async Task DoRegister()
    {
        IsLoading = true;
        (User, Errors) = await Locator.Current.RequireService<UserManager>().Register(UserName, Password, Email);
        if (User?.AuthToken is string token)
        {
            await Locator.Current.RequireService<CredentialStorage>().SaveToken(token);
        }
        IsLoading = false;
    }
}
