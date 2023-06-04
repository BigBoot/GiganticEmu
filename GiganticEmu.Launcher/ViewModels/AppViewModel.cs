using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

    public ReactiveCommand<Unit, Unit> CheckForUpdate { get; }

    public Interaction<AutoUpdater.UpdateInfo, UpdateDialogViewModel.Result> OnUpdateAvailable { get; } = new();
    public Interaction<IObservable<double>, bool> OnUpdate { get; } = new();
    public Interaction<Unit, Unit> OnUpdateFinishing { get; } = new();


    public AppViewModel()
    {
        RestoreUser = ReactiveCommand.CreateFromTask(DoRestoreUserInfo);
        StoreAuthToken = ReactiveCommand.CreateFromTask<string>(DoStoreAuthToken);
        CheckForUpdate = ReactiveCommand.CreateFromTask(DoCheckForUpdate);

        SetupGameVisible = !GameLauncher.GameFound;

        this.WhenAnyValue(
            x => x.User,
            x => x.SettingsVisible,
            x => x.SetupGameVisible,
            (user, settings, setupGame) => (user, settings, setupGame) switch
            {
                { settings: true } => Page.Settings,
                { setupGame: true } => Page.SetupGame,
                { user: { } } => Page.Main,
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
        if (await Locator.Current.RequireService<CredentialStorage>().LoadToken() is string token)
        {
            (User, _) = await Locator.Current.RequireService<UserManager>().Login(token);
        }
        IsLoading = false;
    }

    private async Task DoStoreAuthToken(string token)
    {
        IsLoading = true;
        await Locator.Current.RequireService<CredentialStorage>().SaveToken(token);
        IsLoading = false;
    }

    private async Task DoCheckForUpdate()
    {
        var updater = Locator.Current.RequireService<AutoUpdater>();
        var config = Locator.Current.RequireService<LauncherConfiguration>();

        if (await updater.IsUpdatePending())
        {
            _ = Task.Run(async () => await updater.ApplyPendingUpdate());
            await OnUpdateFinishing.Handle(Unit.Default);
            return;
        }

        if (await updater.CheckForUpdate(config.ForceUpdate) is { } update)
        {
            var result = await OnUpdateAvailable.Handle(update);

            switch (result)
            {
                case UpdateDialogViewModel.Result.Update:
                    var download = updater.DownloadUpdate(update.Version);
                    await OnUpdate.Handle(download);
                    break;

                case UpdateDialogViewModel.Result.Skip:
                    await updater.SkipUpdate(update.Version);
                    break;

                case UpdateDialogViewModel.Result.Later:
                    await updater.RemindLater(TimeSpan.FromDays(1));
                    break;
            }

        }
    }
}
