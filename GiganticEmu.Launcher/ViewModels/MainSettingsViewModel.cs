
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class MainSettingsViewModel : ReactiveObject, SettingsContainerViewModel.SettingsPageViewModel
{
    [Reactive]
    public Settings.BackgroundImage LauncherBackground { get; set; }

    [Reactive]
    public Settings.Language GameLanguage { get; set; }

    [Reactive]
    public string OfflineName { get; set; }

    public ReactiveCommand<Unit, Unit> Apply { get; }
    public ReactiveCommand<Unit, Unit> Reset { get; }

    public MainSettingsViewModel()
    {
        Apply = ReactiveCommand.CreateFromTask(DoApply);
        Reset = ReactiveCommand.CreateFromTask(DoReset);

        var settings = Locator.Current.RequireService<Settings>();

        LauncherBackground = settings.Background.Value;
        GameLanguage = settings.GameLanguage.Value;
        OfflineName = settings.OfflineName.Value;
    }

    private async Task DoApply()
    {
        var settings = Locator.Current.RequireService<Settings>();

        settings.Background.OnNext(LauncherBackground);
        settings.GameLanguage.OnNext(GameLanguage);
        settings.OfflineName.OnNext(OfflineName);
    }

    private async Task DoReset()
    {
        var settings = Locator.Current.RequireService<Settings>();

        LauncherBackground = settings.Background.Value;
        GameLanguage = settings.GameLanguage.Value;
        OfflineName = settings.OfflineName.Value;
    }
}
