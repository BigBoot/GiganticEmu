using System;
using System.Linq;
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

public partial class InputDialog : ReactiveUserControl<InputDialogViewModel>
{
    private readonly Subject<string?> _result = new();
    private IObservable<string?> Result { get => _result; }

    public InputDialog()
    {
        InitializeComponent();
        this.ViewModel = new InputDialogViewModel();

        ViewModel.OnShowDialog.RegisterHandler(async interaction =>
        {
            interaction.SetOutput(await Result.FirstAsync());
        });


        BtnOk.Click += OnButtonClick;
        BtnCancel.Click += OnButtonClick;

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.Title,
                view => view.Title.Text
            );

            this.Bind(ViewModel,
                viewModel => viewModel.Input,
                view => view.Input.Text
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

    private void OnButtonClick(object? sender, RoutedEventArgs args)
    {
        _result.OnNext(sender == BtnOk ? Input.Text : null);
    }
}
