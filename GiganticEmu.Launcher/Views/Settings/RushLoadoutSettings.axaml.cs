using Avalonia.ReactiveUI;
using System.Collections.Generic;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GiganticEmu.Shared;
using System.Linq;
using PeNet.Header.Net.MetaDataTables;

namespace GiganticEmu.Launcher;

public partial class RushLoadoutSettings : ReactiveUserControl<RushLoadoutSettingsViewModel>
{
    private bool _loading = false;

    public RushLoadoutSettings()
    {
        InitializeComponent();
        ViewModel = new RushLoadoutSettingsViewModel();

        var upgrades = new List<ComboBox> {
            Upgrade1,
            Upgrade2,
            Upgrade3,
            Upgrade4,
            Upgrade5,
            Upgrade6,
            Upgrade7,
            Upgrade8,
            Upgrade9,
            Upgrade10,
        };

        foreach (var upgrade in upgrades)
        {
            upgrade.Items = Enum.GetValues<Skill>()
                .SelectMany(skill => new List<SkillUpgrade>
                {
                    new(skill, UpgradePath.Left, null),
                    new(skill, UpgradePath.Left, UpgradePath.Left),
                    new(skill, UpgradePath.Left, UpgradePath.Right),
                    new(skill, UpgradePath.Right, null),
                    new(skill, UpgradePath.Right, UpgradePath.Left),
                    new(skill, UpgradePath.Right, UpgradePath.Right),
                })
                .ToList();
        }

        Talent.Items = Enum.GetValues<Talent>().Select(talent => new TalentUpgrade(talent));

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel!.RushLoadouts)
                .Subscribe(loadouts =>
                {
                    Loadout.Items = loadouts.Items;
                })
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                    viewModel => viewModel.RushLoadout,
                    view => view.Loadout.SelectedItem
                )
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel!.RushLoadout)
                .Subscribe(loadout =>
                {
                    _loading = true;
                    if (loadout != null)
                    {
                        foreach (var (upgrade, i) in loadout.Upgrades.OfType<SkillUpgrade>().Select((x, i) => (x, i)))
                        {
                            upgrades[i].SelectedItem = upgrade;
                        }
                        Talent.SelectedItem = loadout.Upgrades.OfType<TalentUpgrade>().SingleOrDefault();
                    }
                    _loading = false;
                })
                .DisposeWith(disposables);


            Observable.FromEventPattern(Talent, nameof(ComboBox.SelectionChanged))
                .Do(ev =>
                {
                    if (!_loading)
                    {
                        var args = (SelectionChangedEventArgs)ev.EventArgs;
                        var oldUpgrade = (TalentUpgrade)args.RemovedItems[0]!;
                        var index = this.ViewModel!.RushLoadout!.Upgrades.IndexOf(oldUpgrade);
                        var newUpgrade = (TalentUpgrade)args.AddedItems[0]!;
                        this.ViewModel!.RushLoadout!.Upgrades[index] = newUpgrade;
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);


            foreach (var upgrade in upgrades)
            {
                Observable.FromEventPattern(upgrade, nameof(ComboBox.SelectionChanged))
                    .Do(ev =>
                    {
                        if (!_loading)
                        {
                            var args = (SelectionChangedEventArgs)ev.EventArgs;
                            var oldUpgrade = (SkillUpgrade)args.RemovedItems[0]!;
                            var index = this.ViewModel!.RushLoadout!.Upgrades.IndexOf(oldUpgrade);
                            var newUpgrade = (SkillUpgrade)args.AddedItems[0]!;
                            this.ViewModel!.RushLoadout!.Upgrades[index] = newUpgrade;
                        }
                    })
                    .Subscribe()
                    .DisposeWith(disposables);
            }
        });
    }
}