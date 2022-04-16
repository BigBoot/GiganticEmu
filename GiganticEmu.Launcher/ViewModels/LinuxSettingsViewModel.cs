
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class LinuxSettingsViewModel : ReactiveObject, SettingsContainerViewModel.ISettingsPageViewModel
{
    [Reactive]
    public bool GameModeEnabled { get; set; }

    [Reactive]
    public bool MangoHudEnabled { get; set; }

    [Reactive]
    public Settings.CompatiblityTool CompatiblityTool { get; set; }

    public ReactiveCommand<Unit, Unit> Apply { get; }
    public ReactiveCommand<Unit, Unit> Reset { get; }

    public LinuxSettingsViewModel()
    {
        Apply = ReactiveCommand.CreateFromTask(DoApply);
        Reset = ReactiveCommand.CreateFromTask(DoReset);

        var settings = Locator.Current.RequireService<Settings>();

        GameModeEnabled = settings.LinuxEnableGameMode.Value;
        MangoHudEnabled = settings.LinuxEnableMangoHud.Value;
        CompatiblityTool = settings.LinuxCompatiblityTool.Value;
    }

    private async Task DoApply()
    {
        var settings = Locator.Current.RequireService<Settings>();

        settings.LinuxEnableGameMode.OnNext(GameModeEnabled);
        settings.LinuxEnableMangoHud.OnNext(MangoHudEnabled);
        settings.LinuxCompatiblityTool.OnNext(CompatiblityTool);
    }

    private async Task DoReset()
    {
        var settings = Locator.Current.RequireService<Settings>();

        GameModeEnabled = settings.LinuxEnableGameMode.Value;
        MangoHudEnabled = settings.LinuxEnableMangoHud.Value;
        CompatiblityTool = settings.LinuxCompatiblityTool.Value;
    }
}
