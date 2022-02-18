using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class LoginContainerViewModel : ReactiveObject
{
    public enum Page { Login, Register };

    [Reactive]
    public Page CurrentPage { get; set; } = Page.Login;

    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public UserData? User { get; set; } = null;

}
