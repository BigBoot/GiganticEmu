using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace GiganticEmu.Launcher;

public partial class SetupGame
{
    public SetupGame()
    {
        ViewModel = new SetupGameViewModel();
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(
                x => x.ViewModel!.Progress,
                x => x.ViewModel!.CurrentTask,
                (progress, task) => $"{task}: {(int)(progress * 100)}%"
            )
            .BindTo(this, x => x.ProgressText.Text)
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentTask,
                view => view.ProgressText.Text
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentTask,
                view => view.ProgressContainer.Visibility,
                value => value is not null ? Visibility.Visible : Visibility.Collapsed
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CurrentTask,
                view => view.ButtonInstall.Visibility,
                value => value is null ? Visibility.Visible : Visibility.Collapsed
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Progress,
                view => view.ProgressColumn0.Width,
                value => new GridLength(value, GridUnitType.Star)
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Progress,
                view => view.ProgressColumn1.Width,
                value => new GridLength(1 - value, GridUnitType.Star)
            )
            .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                viewModel => viewModel.CanInstall,
                view => view.ButtonInstall.IsEnabled
            )
            .DisposeWith(disposables);

            this.Bind(ViewModel,
                viewModel => viewModel.InstallPath,
                view => view.InstallPath.Text
            )
            .DisposeWith(disposables);

            this.Bind(ViewModel,
                viewModel => viewModel.ZipPath,
                view => view.ZipPath.Text
            )
            .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonInstall, nameof(ButtonInstall.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.Install)
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonBrowseInstallpath, nameof(ButtonBrowseInstallpath.Click))
                .Select(_ => new VistaFolderBrowserDialog())
                .Do(dlg =>
                {

                    if (dlg.ShowDialog() == true)
                    {
                        ViewModel.InstallPath = dlg.SelectedPath switch
                        {
                          var path when Directory.Exists(path) && new DirectoryInfo(path).EnumerateFileSystemInfos().Any() => Path.Join(path, "Gigantic"),
                          var path => path,
                        };
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonBrowseZipPath, nameof(ButtonBrowseZipPath.Click))
                .Select(_ => new VistaOpenFileDialog())
                .Do(dlg =>
                {
                    dlg.Filter = "Gigantic-Core_de.zip|Gigantic-Core_de.zip";
                    if (dlg.ShowDialog() == true)
                    {
                        ViewModel.ZipPath = dlg.FileName;
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            ViewModel.VerificationFailed.RegisterHandler(async interaction =>
            {
                using var dlg = new TaskDialog();
                dlg.WindowTitle = interaction.Input;
                dlg.MainIcon = TaskDialogIcon.Error;
                dlg.MainInstruction =
                    "The selected file could not be verified.";
                dlg.Content =
                    "This either means the file got corrupted while downloading or it's not the correct file.";
                dlg.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                dlg.ShowDialog();

                interaction.SetOutput(Unit.Default);
            });

            ViewModel.InstallationFailed.RegisterHandler(async interaction =>
            {
                using var dlg = new TaskDialog();
                dlg.WindowTitle = "Installation failed";
                dlg.MainIcon = TaskDialogIcon.Error;
                dlg.MainInstruction =
                    "Installation failed.";
                dlg.Content = interaction.Input;
                dlg.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                dlg.ShowDialog();

                interaction.SetOutput(Unit.Default);
            });

            ViewModel.CreateShortcut.RegisterHandler(async interaction =>
            {
                using var dlg = new TaskDialog();
                dlg.WindowTitle = "Installation finished";
                dlg.MainInstruction =
                    "The installation finished successfully.";
                dlg.Content =
                    "Do you want to create a desktop shortcut for Mistforge Launcher?";

                var yes = new TaskDialogButton(ButtonType.Yes);
                var no = new TaskDialogButton(ButtonType.No);

                dlg.Buttons.Add(yes);
                dlg.Buttons.Add(no);

                interaction.SetOutput(dlg.ShowDialog() == yes);
            });

            DiscordLink.RequestNavigate += (_, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            };
        });
    }
}
