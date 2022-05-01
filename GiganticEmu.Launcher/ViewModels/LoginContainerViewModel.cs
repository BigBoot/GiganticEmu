using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class LoginContainerViewModel : ReactiveObject
{
    public enum Page { Login, Register, ForgotPassword, ResetPassword };

    [Reactive]
    public Page CurrentPage { get; set; } = Page.Login;

    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public string UserName { get; set; } = "";

    [Reactive]
    public UserData? User { get; set; } = null;

}
