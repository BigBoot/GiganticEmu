using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class ForgotPasswordViewModel : ReactiveObject
{
    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public string UserName { get; set; } = "";

    [Reactive]
    public ICollection<string> Errors { get; set; } = new List<string>();

    public ReactiveCommand<Unit, Unit> Reset { get; }

    public Interaction<Unit, Unit> OnResetSuccesfull { get; } = new();

    public ForgotPasswordViewModel()
    {
        Reset = ReactiveCommand.CreateFromTask(DoReset);
    }

    private async Task DoReset()
    {
        IsLoading = true;
        Errors = (await Locator.Current.RequireService<UserManager>().ResetPassword(UserName)).Errors;
        if (Errors.Count == 0)
        {
            await OnResetSuccesfull.Handle(Unit.Default);
        }
        IsLoading = false;
    }
}
