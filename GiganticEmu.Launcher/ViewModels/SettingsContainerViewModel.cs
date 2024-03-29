﻿using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class SettingsContainerViewModel : ReactiveObject
{
    public interface ISettingsPageViewModel
    {
        public ReactiveCommand<Unit, Unit> Apply { get; }
        public ReactiveCommand<Unit, Unit> Reset { get; }
    }

    [Reactive]
    public bool SettingsVisible { get; set; } = false;

    [Reactive]
    public ICollection<ISettingsPageViewModel> Pages { get; set; } = new List<ISettingsPageViewModel>();

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
        await Locator.Current.RequireService<Settings>().Save();
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
