using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace GiganticEmu.Launcher;

public partial class LoginContainer
{
    public LoginContainer()
    {
        ViewModel = new LoginContainerViewModel();
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.User,
                view => view.PageLogin.ViewModel!.User
            );

            this.Bind(ViewModel,
                viewModel => viewModel.User,
                view => view.PageRegister.ViewModel!.User
            );

            Observable.FromEventPattern(PageLogin, nameof(Login.RegisterClicked))
                .Do(_ => ContentContainer.SelectedIndex = 1)
                .Subscribe()
                .DisposeWith(disposables);

            Observable.FromEventPattern(PageRegister, nameof(Register.LoginClicked))
                .Do(_ => ContentContainer.SelectedIndex = 0)
                .Subscribe()
                .DisposeWith(disposables);
        });
    }
}
