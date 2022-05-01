using ReactiveUI;
using System;
using Avalonia.ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive;

namespace GiganticEmu.Launcher;

public partial class ForgotPassword : ReactiveUserControl<ForgotPasswordViewModel>
{
    public event EventHandler? CancelClicked;
    public event EventHandler? ResetSuccesfull;

    public ForgotPassword()
    {
        InitializeComponent();
        ViewModel = new ForgotPasswordViewModel();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.UserName,
                view => view.TextUserName.Text
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
                viewModel => viewModel.Reset,
                view => view.ButtonReset
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


            ViewModel.OnResetSuccesfull.RegisterHandler(async interaction =>
            {
                if (ResetSuccesfull is EventHandler handler)
                {
                    handler.Invoke(this, new EventArgs());
                }
                interaction.SetOutput(Unit.Default);
            });

        });
    }
}