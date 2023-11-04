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

public partial class Connect : ReactiveUserControl<ConnectViewModel>
{
    public Connect()
    {
        InitializeComponent();
        ViewModel = new ConnectViewModel();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                    viewModel => viewModel.UserName,
                    view => view.TextName.Text
                )
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                    viewModel => viewModel.Host,
                    view => view.TextHost.Text
                )
                .DisposeWith(disposables);

            this.BindCommand(ViewModel,
                viewModel => viewModel.Connect,
                view => view.ButtonConnect
            );

            ViewModel.OnConfirm.RegisterHandler(async interaction =>
            {
                var result = await this.ShowAlertDialog(async dialog =>
                {
                    dialog.Title = interaction.Input.Title;
                    dialog.Text = interaction.Input.Text;
                    dialog.Icon = "HelpCircleOutline";

                    dialog.Buttons.Edit(buttons =>
                    {
                        buttons.Add(new AlertDialogViewModel.Button("Yes", "yes"));
                        buttons.Add(new AlertDialogViewModel.Button("No", "no"));
                    });
                });

                interaction.SetOutput(result == "yes");
            });

            ViewModel.OnError.RegisterHandler(async interaction =>
            {
                var result = await this.ShowAlertDialog(async dialog =>
                {
                    dialog.Title = interaction.Input.Title;
                    dialog.Text = interaction.Input.Text;
                    dialog.Icon = "AlertCircleOutline";

                    dialog.Buttons.Edit(buttons =>
                    {
                        buttons.Add(new AlertDialogViewModel.Button("Ok", "ok"));
                    });
                });

                interaction.SetOutput(Unit.Default);
            });
        });
    }
}
