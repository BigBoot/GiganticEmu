
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class MainSettingsViewModel : ReactiveObject, SettingsContainerViewModel.SettingsPageViewModel
{
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

        GameLanguage = Settings.GameLanguage;
        OfflineName = Settings.OfflineName;
    }

    private async Task DoApply()
    {
        Settings.GameLanguage = GameLanguage;
        Settings.OfflineName = OfflineName;
    }

    private async Task DoReset()
    {
        GameLanguage = Settings.GameLanguage;
        OfflineName = Settings.OfflineName;
    }
}
