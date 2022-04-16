using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace GiganticEmu.Launcher;

public partial class MainContainer : ReactiveUserControl<MainContainerViewModel>
{
    public MainContainer()
    {
        InitializeComponent();
        ViewModel = new MainContainerViewModel();

        this.WhenActivated(disposables =>
        {
            Observable.FromEventPattern(ButtonStartGame, nameof(ButtonStartGame.Click))
                .Select(x => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.StartGame)
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