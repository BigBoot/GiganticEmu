using MaterialDesignExtensions.Controls;
using ReactiveUI;
using System.Windows;

namespace GiganticEmu.Launcher;

public abstract class ReactiveMaterialWindow<TViewModel> : MaterialWindow, IViewFor<TViewModel>
    where TViewModel : class
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            "ViewModel",
            typeof(TViewModel),
            typeof(ReactiveUserControl<TViewModel>),
            new PropertyMetadata(null));


    public TViewModel? BindingRoot => ViewModel;

    public TViewModel? ViewModel
    {
        get => (TViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }
}
