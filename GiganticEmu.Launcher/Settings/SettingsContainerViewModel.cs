using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class SettingsContainerViewModel : ReactiveObject
{
    public interface SettingsPageViewModel
    {
        public ReactiveCommand<Unit, Unit> Apply { get; }
        public ReactiveCommand<Unit, Unit> Reset { get; }
    }

    [Reactive]
    public bool SettingsVisible { get; set; } = false;

    [Reactive]
    public ICollection<SettingsPageViewModel> Pages { get; set; } = new List<SettingsPageViewModel>();

    public ReactiveCommand<Unit, Unit> SaveSettings { get; }

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public SettingsContainerViewModel()
    {
        SaveSettings = ReactiveCommand.CreateFromTask(DoSaveSettings);
        Cancel = ReactiveCommand.CreateFromTask(DoCancel);
    }

    private async Task DoSaveSettings()
    {
        foreach (var page in Pages)
        {
            await page.Apply.Execute();
        }
        Settings.Save();
        SettingsVisible = false;
    }

    private async Task DoCancel()
    {
        foreach (var page in Pages)
        {
            await page.Reset.Execute();
        }
        SettingsVisible = false;
    }
}
