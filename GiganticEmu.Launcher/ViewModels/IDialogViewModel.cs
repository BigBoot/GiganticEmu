using System.Reactive;
using ReactiveUI;

namespace GiganticEmu.Launcher;

public interface IDialogViewModel<T>
{

    public ReactiveCommand<Unit, T?> Show { get; }
}