using System.Collections.Generic;
using Avalonia.Controls;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace GiganticEmu.Launcher;

public static class AvaloniaExtensions
{
    public static IEnumerable<IControl> Parents(this IControl control)
    {
        var current = control.Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public static Window? GetWindow(this IControl control)
    {
        return control.Parents().Prepend(control).OfType<Window>().FirstOrDefault();
    }

    public static MainWindow? GetMainWindow(this IControl control)
    {
        return control.Parents().Prepend(control).OfType<MainWindow>().FirstOrDefault();
    }

    private static async Task<TResult?> ShowDialog<TResult, TViewModel>(IControl control, ReactiveUserControl<TViewModel> dialog, Func<TViewModel, Task> setup)
        where TViewModel : class, IDialogViewModel<TResult>
    {
        if (control.GetMainWindow() is MainWindow window)
        {
            await setup((dialog.ViewModel as TViewModel)!);
            window.DialogContent.Children.Add(dialog);
            var result = await dialog.ViewModel!.Show.Execute();

            window.DialogContent.Children.Remove(dialog);

            return result;
        }

        return default(TResult);
    }

    public static async Task<string?> ShowAlertDialog(this IControl control, Func<AlertDialogViewModel, Task> setup)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return await ShowDialog<string, AlertDialogViewModel>(control, new AlertDialog(), setup);
        });
    }

    public static async Task<string?> ShowInputDialog(this IControl control, Func<InputDialogViewModel, Task> setup)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return await ShowDialog<string, InputDialogViewModel>(control, new InputDialog(), setup);
        });
    }

    public static async Task<UpdateDialogViewModel.Result> ShowUpdateDialog(this IControl control, Func<UpdateDialogViewModel, Task> setup)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return await ShowDialog<UpdateDialogViewModel.Result, UpdateDialogViewModel>(control, new UpdateDialog(), setup);
        });
    }

    public static async Task<bool> ShowProgressDialog(this IControl control, Func<ProgressDialogViewModel, Task> setup)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return await ShowDialog<bool, ProgressDialogViewModel>(control, new ProgressDialog(), setup);
        });
    }

    public static void Shutdown(this Application application, int exitCode = 0)
    {
        if (application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.Post(() => desktop.Shutdown(exitCode));
        }
    }
}