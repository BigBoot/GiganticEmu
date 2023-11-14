
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using DynamicData;
using GiganticEmu.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class RushLoadoutSettingsViewModel : ReactiveObject, SettingsContainerViewModel.ISettingsPageViewModel
{
    [Reactive]
    public RushLoadout? RushLoadout { get; set; }

    public ISourceList<RushLoadout> RushLoadouts { get; } = new SourceList<RushLoadout>();

    public ReactiveCommand<Unit, Unit> Apply { get; }
    public ReactiveCommand<Unit, Unit> Reset { get; }

    public RushLoadoutSettingsViewModel()
    {
        Apply = ReactiveCommand.CreateFromTask(DoApply);
        Reset = ReactiveCommand.CreateFromTask(DoReset);

        _ = Task.Run(DoReset);
    }

    private async Task DoApply()
    {
        if (RushLoadout is RushLoadout loadout)
        {
            var configuration = Locator.Current.RequireService<LauncherConfiguration>();
            await GameUtils.SaveRushLoadout(loadout, configuration.Game);
        }
    }

    private async Task DoReset()
    {
        var configuration = Locator.Current.RequireService<LauncherConfiguration>();
        var loadouts = (await GameUtils.GetRushLoadouts(configuration.Game)).ToList();

        RushLoadouts.Clear();
        RushLoadouts.AddRange(loadouts);
        RushLoadout = RushLoadouts.Items.First();
    }
}
