using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace GiganticEmu.Launcher;

public partial class SetupGame : ReactiveUserControl<SetupGameViewModel>
{
    public SetupGame()
    {
        InitializeComponent();
        ViewModel = new SetupGameViewModel();

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
                    view => view.ProgressContainer.IsVisible,
                    value => value is not null
                )
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.CurrentTask,
                    view => view.ButtonBrowseInstallpath.IsEnabled,
                    value => value is null
                )
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.CurrentTask,
                    view => view.ButtonBrowseZipPath.IsEnabled,
                    value => value is null
                )
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.CurrentTask,
                    view => view.ZipPath.IsEnabled,
                    value => value is null
                )
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.CurrentTask,
                    view => view.InstallPath.IsEnabled,
                    value => value is null
                )
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.CurrentTask,
                    view => view.ButtonInstall.IsVisible,
                    value => value is null
                )
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.Progress,
                    view => view.Progress.Value
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
                .Select(_ => new OpenFolderDialog
                {
                    Title = "Select installation directory",
                })
                .Do(dlg =>
                {
                    if (this.GetWindow() is Window window)
                    {
                        ViewModel.InstallPath = dlg.ShowAsync(window).Result switch
                        {
                            string path when Directory.Exists(path) &&
                                             new DirectoryInfo(path).EnumerateFileSystemInfos().Any() => Path.Join(path,
                                "Gigantic"),
                            string path => path,
                            _ => ViewModel.InstallPath,
                        };
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonBrowseZipPath, nameof(ButtonBrowseZipPath.Click))
                .Select(_ => new OpenFileDialog
                {
                    Filters =
                    {
                        new FileDialogFilter {Name = "Gigantic-Core_de.zip", Extensions = {"zip"}}
                    }
                })
                .Do(dlg =>
                {
                    if (this.GetWindow() is Window window)
                    {
                        if (dlg.ShowAsync(window).Result is { Length: > 0 } files)
                        {
                            ViewModel.ZipPath = files[0];
                        }
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            ViewModel.VerificationFailed.RegisterHandler(async interaction =>
            {
                await this.ShowAlertDialog(async dialog =>
                {
                    dialog.Title = "The selected file could not be verified.";
                    dialog.Text = "This either means the file got corrupted while downloading or it's not the correct file.";
                    dialog.Icon = "AlertCircleOutline";

                    dialog.Buttons.Edit(buttons =>
                    {
                        buttons.Add(new AlertDialogViewModel.Button("Ok", "ok"));
                    });
                });

                interaction.SetOutput(Unit.Default);
            });

            ViewModel.InstallationFailed.RegisterHandler(async interaction =>
            {
                await this.ShowAlertDialog(async dialog =>
               {
                   dialog.Title = "Installation failed";
                   dialog.Text = interaction.Input;
                   dialog.Icon = "AlertCircleOutline";

                   dialog.Buttons.Edit(buttons =>
                   {
                       buttons.Add(new AlertDialogViewModel.Button("Ok", "ok"));
                   });
               });

                interaction.SetOutput(Unit.Default);
            });

            ViewModel.CreateShortcut.RegisterHandler(async interaction =>
            {
                var result = await this.ShowAlertDialog(async dialog =>
               {
                   dialog.Title = "Installation finished";
                   dialog.Text = "The installation finished successfully.\nDo you want to create a desktop shortcut for Mistforge Launcher?";
                   dialog.Icon = "HelpCircleOutline";

                   dialog.Buttons.Edit(buttons =>
                   {
                       buttons.Add(new AlertDialogViewModel.Button("Yes", "yes"));
                       buttons.Add(new AlertDialogViewModel.Button("No", "no"));
                   });
               });

                interaction.SetOutput(result == "yes");
            });

            ViewModel.Quit.RegisterHandler(async interaction =>
            {
                App.Current?.Shutdown(0);
            });
        });
    }
}