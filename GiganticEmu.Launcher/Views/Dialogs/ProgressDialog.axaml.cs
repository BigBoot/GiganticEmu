using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Material.Icons;
using ReactiveUI;

namespace GiganticEmu.Launcher;

public partial class ProgressDialog : ReactiveUserControl<ProgressDialogViewModel>
{
    private readonly Subject<bool> _result = new();
    private IObservable<bool> Result { get => _result; }

    public ProgressDialog()
    {
        InitializeComponent();
        this.ViewModel = new ProgressDialogViewModel();

        ViewModel.OnShowDialog.RegisterHandler(async interaction =>
        {
            interaction.SetOutput(await Result.FirstAsync());
        });

        ViewModel.OnFinished.RegisterHandler(async interaction =>
        {
            _result.OnNext(true);
            interaction.SetOutput(Unit.Default);
        });


        BtnCancel.Click += (_, _) => _result.OnNext(false);

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.Title,
                view => view.Title.Text
            );

            this.Bind(ViewModel,
                viewModel => viewModel.Value,
                view => view.Progress.Value
            );

            this.Bind(ViewModel,
                viewModel => viewModel.IsIndeterminate,
                view => view.Progress.IsIndeterminate
            );

            this.Bind(ViewModel,
                viewModel => viewModel.CanCancel,
                view => view.BtnCancel.IsVisible
            );

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Icon,
                view => view.Icon.Kind,
                value => Enum.Parse<MaterialIconKind>(value ?? "Abacus")
            );

            this.OneWayBind(ViewModel,
                viewModel => viewModel.IconColor,
                view => view.Icon.Foreground,
                value => value is not null ? new SolidColorBrush(((uint)value.Value.ToArgb())) : App.Current!.FindResource("MaterialDesignBody")
            );

            this.OneWayBind(ViewModel,
                viewModel => viewModel.IconSize,
                view => view.Icon.Width
            );

            this.OneWayBind(ViewModel,
                viewModel => viewModel.IconSize,
                view => view.Icon.Height
            );

            this.OneWayBind(ViewModel,
                viewModel => viewModel.Icon,
                view => view.Icon.IsVisible,
                value => value is not null
            );
        });
    }
}
