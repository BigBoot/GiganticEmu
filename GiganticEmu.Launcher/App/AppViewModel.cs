using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class AppViewModel : ReactiveObject
{
    public enum Page
    {
        Login,
        Main, 
        Settings,
        SetupGame,
    }

    [Reactive]
    public bool IsLoading { get; set; }

    [Reactive]
    public UserData? User { get; set; }

    [Reactive]
    public bool SettingsVisible { get; set; }

    [Reactive] 
    public bool SetupGameVisible { get; set; }

    [ObservableAsProperty]
    public Page CurrentPage { get; } = Page.Login;

    [ObservableAsProperty] 
    public string PageTitle { get; } = "";

    public ReactiveCommand<Unit, Unit> RestoreUser { get; }

    public ReactiveCommand<string, Unit> StoreAuthToken { get; }


    public AppViewModel()
    {
        RestoreUser = ReactiveCommand.CreateFromTask(DoRestoreUserInfo);
        StoreAuthToken = ReactiveCommand.CreateFromTask<string>(DoStoreAuthToken);

        SetupGameVisible = !GameLauncher.GameFound;

        this.WhenAnyValue(
            x => x.User,
            x => x.SettingsVisible,
            x => x.SetupGameVisible,
            (user, settings, setupGame) => (user, settings, setupGame) switch
            {
                { settings: true } => Page.Settings,
                { setupGame: true } => Page.SetupGame,
                { user: {} } => Page.Main,
                _ => Page.Login,
            }
        ).ToPropertyEx(this, x => x.CurrentPage);

        this.WhenAnyValue(
            x => x.CurrentPage,
            page => page switch
            {
                Page.Settings => "Settings",
                Page.SetupGame => "Gigantic Heaven Setup",
                _ => "Mistforge Launcher",
            }
        ).ToPropertyEx(this, x => x.PageTitle);
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
