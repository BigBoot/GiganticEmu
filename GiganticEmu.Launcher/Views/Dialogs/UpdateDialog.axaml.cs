using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace GiganticEmu.Launcher;

public partial class UpdateDialog : ReactiveUserControl<UpdateDialogViewModel>
{
    private readonly Subject<UpdateDialogViewModel.Result> _result = new();
    private IObservable<UpdateDialogViewModel.Result> Result { get => _result; }

    public UpdateDialog()
    {
        InitializeComponent();
        this.ViewModel = new UpdateDialogViewModel();

        BtnLater.Click += (_, _) => _result.OnNext(UpdateDialogViewModel.Result.Later);
        BtnSkip.Click += (_, _) => _result.OnNext(UpdateDialogViewModel.Result.Skip);
        BtnUpdate.Click += (_, _) => _result.OnNext(UpdateDialogViewModel.Result.Update);

        ViewModel.OnShowDialog.RegisterHandler(async interaction =>
        {
            interaction.SetOutput(await Result.FirstAsync());
        });

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel,
                viewModel => viewModel.Changelog,
                view => view.Changes.Items,
                value => value.versions
            );
        });
    }
}
