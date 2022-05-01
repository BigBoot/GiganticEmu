using ReactiveUI;
using System;
using Avalonia.ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Avalonia.Controls;
using System.Reactive;

namespace GiganticEmu.Launcher;

public partial class Login : ReactiveUserControl<LoginViewModel>
{
    public event EventHandler? RegisterClicked;
    public event EventHandler? ForgotPasswordClicked;


    public Login()
    {
        InitializeComponent();
        ViewModel = new LoginViewModel();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.UserName,
                view => view.TextUserName.Text
            );

            this.Bind(ViewModel,
                viewModel => viewModel.Password,
                view => view.TextPassword.Text
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
                viewModel => viewModel.Login,
                view => view.ButtonLogin
            );

            this.BindCommand(ViewModel,
                viewModel => viewModel.StartOffline,
                view => view.ButtonStartOffline
            );

            Observable.FromEventPattern(ButtonRegister, nameof(ButtonLogin.Click))
                .Do(_ =>
                {
                    if (RegisterClicked is EventHandler handler)
                    {
                        handler.Invoke(this, new EventArgs());
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            Observable.FromEventPattern(ButtonResetPassword, nameof(ButtonResetPassword.Click))
                .Do(_ =>
                {
                    if (ForgotPasswordClicked is EventHandler handler)
                    {
                        handler.Invoke(this, new EventArgs());
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

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