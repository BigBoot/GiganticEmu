using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GiganticEmu.Shared;

namespace GiganticEmu.ModdingToolkit;

public class ModEntry
{
    public required Mod Mod { get; init; }
    public bool Enabled { get; set; } = false;
}

public partial class MainWindow : Window
{
    private ModLoader modLoader = new();
    private ModUtils modUtils = new();

    public void OnModClicked(object source, PointerPressedEventArgs args)
    {
        var check = (source as Panel)?.Children?.OfType<CheckBox>().FirstOrDefault();
        if (check != null)
        {
            check.IsChecked = !check.IsChecked;
        }
    }

    public void OnApplyClicked(object source, RoutedEventArgs args)
    {
        var mods = this.mods.ItemsSource!
            .OfType<ModEntry>()
            .Where(x => x.Enabled)
            .Select(x => x.Mod);

        Task.Run(async () =>
       {
           await Dispatcher.UIThread.InvokeAsync(() =>
           {
               dialogText.Text = $"Applying mods...";
               dialogOk.IsEnabled = false;
               dialog.IsOpen = true;
           });

           await modUtils.ApplyMods(mods, (cur, max) =>
           {
               Dispatcher.UIThread.InvokeAsync(() =>
               {
                   dialogProgress.Maximum = max;
                   dialogProgress.Value = cur;
                   dialogProgress.IsVisible = true;
               });
           }, (level, msg) =>
           {
               Dispatcher.UIThread.InvokeAsync(() =>
                {
                    dialogText.Text = msg;
                });
               Console.Out.WriteLine($"[{level}] {msg}");
           });

           await Dispatcher.UIThread.InvokeAsync(() =>
           {
               dialogText.Text = $"Done...";
               dialogProgress.Value = dialogProgress.Maximum;
               dialogOk.IsEnabled = true;
           });

           modUtils.ClearCache();

           await new AsyncEvent<RoutedEventArgs>(x => dialogOk.Click += x, x => dialogOk.Click -= x).Task;

           await Dispatcher.UIThread.InvokeAsync(() =>
           {
               dialog.IsOpen = false;
               dialogProgress.IsVisible = false;
           });
       });
    }

    public void OnRestoreBackupsClicked(object source, RoutedEventArgs args)
    {
        Task.Run(async () =>
        {
            var restored = await modUtils.RestoreBackups();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialogText.Text = $"Restored {restored.Count()} packages.";
                dialog.IsOpen = true;
            });

            modUtils.ClearCache();

            await new AsyncEvent<RoutedEventArgs>(x => dialogOk.Click += x, x => dialogOk.Click -= x).Task;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialogText.Text = "";
                dialog.IsOpen = false;
            });
        });
    }

    public void OnDecompressAllClicked(object source, RoutedEventArgs args)
    {
        Task.Run(async () =>
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialogText.Text = $"Decompressing packages...";
                dialogOk.IsEnabled = false;
                dialog.IsOpen = true;
            });

            var decompiled = await modUtils.DecompressAll((cur, max) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    dialogProgress.Maximum = max;
                    dialogProgress.Value = cur;
                    dialogProgress.IsVisible = true;
                });
            });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialogProgress.Value = dialogProgress.Maximum;
                dialogOk.IsEnabled = true;
            });

            modUtils.ClearCache();

            await new AsyncEvent<RoutedEventArgs>(x => dialogOk.Click += x, x => dialogOk.Click -= x).Task;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialog.IsOpen = false;
                dialogProgress.IsVisible = false;
            });
        });
    }

    public void OnDecompileAllClicked(object source, RoutedEventArgs args)
    {
        Task.Run(async () =>
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialogText.Text = $"Decompiling packages...";
                dialogOk.IsEnabled = false;
                dialog.IsOpen = true;
            });

            var decompiled = await modUtils.DecompileAll((cur, max) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    dialogProgress.Maximum = max;
                    dialogProgress.Value = cur;
                    dialogProgress.IsVisible = true;
                });
            });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialogProgress.Value = dialogProgress.Maximum;
                dialogOk.IsEnabled = true;
            });

            await new AsyncEvent<RoutedEventArgs>(x => dialogOk.Click += x, x => dialogOk.Click -= x).Task;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                dialog.IsOpen = false;
                dialogProgress.IsVisible = false;
            });
        });
    }

    public MainWindow()
    {
#if DEBUG
        this.AttachDevTools(new KeyGesture(Key.F11));
#endif

        InitializeComponent();

        Task.Run(async () =>
        {
            var build = await GameUtils.GetGameBuild();
            var mods = await modLoader.LoadMods();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.mods.ItemsSource = new ObservableCollection<ModEntry>(
                    mods
                        .Where(x => x.Builds.Contains(build))
                        .Select(x => new ModEntry { Mod = x })
                );
            });
        });

    }
}