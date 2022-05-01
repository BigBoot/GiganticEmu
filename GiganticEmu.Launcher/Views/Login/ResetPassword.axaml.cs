using ReactiveUI;
using System;
using Avalonia.ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Avalonia.Controls;
using System.Reactive;

namespace GiganticEmu.Launcher;

public partial class ResetPassword : ReactiveUserControl<ResetPasswordViewModel>
{
    public event EventHandler? CancelClicked;

    public ResetPassword()
    {
        InitializeComponent();
        ViewModel = new ResetPasswordViewModel();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.Password,
                view => view.TextPassword.Text
            );

            this.Bind(ViewModel,
                viewModel => viewModel.Token,
                view => view.TextToken.Text
            );

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Errors,
                view => view.TextError.Text,
                value => string.Join('\n', value)
            );

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Errors,
                view => view.TextError.IsVisible,
                value => value.Count != 0
            );

            this.BindCommand(ViewModel,
                viewModel => viewModel.ChangePassword,
                view => view.ButtonChangePassword
            );

            Observable.FromEventPattern(ButtonCancel, nameof(ButtonCancel.Click))
                .Do(_ =>
                {
                    if (CancelClicked is EventHandler handler)
                    {
                        handler.Invoke(this, new EventArgs());
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);


            ViewModel.OnSuccess.RegisterHandler(async interaction =>
            {
                var result = await this.ShowAlertDialog(async dialog =>
                {
                    dialog.Title = "Password changed successful!";
                    dialog.Text = "The password has been changed sucesfully, you can now log in using the new password.";
                    dialog.Icon = "AlertCircleOutline";

                    dialog.Buttons.Edit(buttons =>
                    {
                        buttons.Add(new AlertDialogViewModel.Button("Ok", "ok"));
                    });
                });

                interaction.SetOutput(Unit.Default);

                if (CancelClicked is EventHandler handler)
                {
                    handler.Invoke(this, new EventArgs());
                }
            });
        });
    }
}