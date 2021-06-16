using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GiganticEmu.Launcher
{
    public partial class Register
    {
        public event EventHandler? LoginClicked;

        public Register()
        {
            ViewModel = new RegisterViewModel();
            InitializeComponent();

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
                    view => view.TextError.Visibility,
                    value => value.Count == 0 ? Visibility.Hidden : Visibility.Visible
                );

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Register,
                    view => view.ButtonRegister
                );

                Observable.FromEventPattern(TextPassword, nameof(TextPassword.PasswordChanged))
                    .Select(x => ((PasswordBox)x.Sender!).Password)
                    .Do(password => ViewModel.Password = password)
                    .Subscribe()
                    .DisposeWith(disposables);

                Observable.FromEventPattern(TextPasswordConfirm, nameof(TextPassword.PasswordChanged))
                    .Select(x => ((PasswordBox)x.Sender!).Password)
                    .Do(password => ViewModel.PasswordConfirm = password)
                    .Subscribe()
                    .DisposeWith(disposables);

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
}
