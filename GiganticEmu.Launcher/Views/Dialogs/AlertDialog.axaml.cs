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

public partial class AlertDialog : ReactiveUserControl<AlertDialogViewModel>
{
    private readonly Subject<string?> _result = new();
    private IObservable<string?> Result { get => _result; }

    public AlertDialog()
    {
        InitializeComponent();
        this.ViewModel = new AlertDialogViewModel();

        ViewModel.OnShowDialog.RegisterHandler(async interaction =>
        {
            interaction.SetOutput(await Result.FirstAsync());
        });

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel,
                viewModel => viewModel.Title,
                view => view.Title.Text
            );

            this.Bind(ViewModel,
                viewModel => viewModel.Text,
                view => view.Text.Text
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

            this.WhenAnyValue(x => x.ViewModel!.Buttons)
                .Subscribe(buttons =>
                {
                    var children = buttons.Items.Select(button => new Button()
                    {
                        Content = button.Text,
                        Tag = button.Value,
                        Classes = new Classes(new[] { "Flat" }),
                    })
                    .Do(button => button.Click += OnButtonClick);

                    Buttons.Children.Clear();
                    foreach (var child in children)
                    {
                        Buttons.Children.Add(child);
                    }
                })
                .DisposeWith(disposables);
        });
    }

    private void OnButtonClick(object? sender, RoutedEventArgs args)
    {
        _result.OnNext((sender as Control)?.Tag as string);
    }
}
