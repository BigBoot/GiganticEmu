using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class ResetPasswordViewModel : ReactiveObject
{
    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public string UserName { get; set; } = "";

    [Reactive]
    public string Token { get; set; } = "";

    [Reactive]
    public string Password { get; set; } = "";

    [Reactive]
    public ICollection<string> Errors { get; set; } = new List<string>();

    public ReactiveCommand<Unit, Unit> ChangePassword { get; }

    public Interaction<Unit, Unit> OnSuccess { get; } = new();

    public ResetPasswordViewModel()
    {
        ChangePassword = ReactiveCommand.CreateFromTask(DoChangePassword);
    }

    private async Task DoChangePassword()
    {
        IsLoading = true;
        Errors = (await Locator.Current.RequireService<UserManager>().ChangePassword(UserName, Token, Password)).Errors;
        System.Console.Out.WriteLine(UserName);
        System.Console.Out.WriteLine(Token);
        System.Console.Out.WriteLine(Password);
        if (Errors.Count == 0)
        {
            await OnSuccess.Handle(Unit.Default);
        }
        IsLoading = false;
    }
}
