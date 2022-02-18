using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;


namespace GiganticEmu.Launcher;

public partial class Register : ReactiveUserControl<RegisterViewModel>
{
    public event EventHandler? LoginClicked;

    public Register()
    {
        InitializeComponent();
        ViewModel = new RegisterViewModel();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.UserName,
                view => view.TextUserName.Text
            );

            this.Bind(ViewModel,
                viewModel => viewModel.Email,
                view => view.TextEmail.Text
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
                viewModel => viewModel.Register,
                view => view.ButtonRegister
            );

            this.Bind(ViewModel,
                viewModel => viewModel.Password,
                view => view.TextPassword.Text
            );

            this.Bind(ViewModel,
                viewModel => viewModel.PasswordConfirm,
                view => view.TextPasswordConfirm.Text
            );

            Observable.FromEventPattern(ButtonLogin, nameof(ButtonLogin.Click))
                .Do(_ =>
                {
                    if (LoginClicked is EventHandler handler)
                    {
                        handler.Invoke(this, new EventArgs());
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);
        });
    }
}