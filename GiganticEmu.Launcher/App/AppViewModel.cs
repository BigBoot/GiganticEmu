using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class AppViewModel : ReactiveObject
{

    [Reactive]
    public bool IsLoading { get; set; } = false;

    [Reactive]
    public UserData? User { get; set; } = null;

    public ReactiveCommand<Unit, Unit> RestoreUser { get; }

    public ReactiveCommand<string, Unit> StoreAuthToken { get; }


    public AppViewModel()
    {
        RestoreUser = ReactiveCommand.CreateFromTask(DoRestoreUserInfo);
        StoreAuthToken = ReactiveCommand.CreateFromTask<string>(DoStoreAuthToken);
    }

    private async Task DoRestoreUserInfo()
    {
        IsLoading = true;
        if (await Locator.Current.GetService<CredentialStorage>()!.LoadToken() is string token)
        {
            (User, _) = await Locator.Current.GetService<UserManager>()!.Login(token);
        }
        IsLoading = false;
    }

    private async Task DoStoreAuthToken(string token)
    {
        IsLoading = true;
        await Locator.Current.GetService<CredentialStorage>()!.SaveToken(token);
        IsLoading = false;
    }
}
