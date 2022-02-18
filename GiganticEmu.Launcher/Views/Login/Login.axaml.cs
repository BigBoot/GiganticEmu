using ReactiveUI;
using System;
using Avalonia.ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Avalonia.Controls;

namespace GiganticEmu.Launcher;

public partial class Login : ReactiveUserControl<LoginViewModel>
{
    public event EventHandler? RegisterClicked;

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
        });
    }
}