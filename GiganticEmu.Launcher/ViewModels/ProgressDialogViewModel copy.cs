using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class ProgressDialogViewModel : ReactiveObject, IDialogViewModel<bool>
{
    [Reactive]
    public string Title { get; set; } = "Title";

    [Reactive]
    public double Value { get; set; } = 0.0;

    [Reactive]
    public bool IsIndeterminate { get; set; } = false;

    [Reactive]
    public bool CanCancel { get; set; } = false;

    [Reactive]
    public string? Icon { get; set; }

    [Reactive]
    public float IconSize { get; set; } = 32;

    [Reactive]
    public Color? IconColor { get; set; }

    [Reactive]
    public bool CanClickAway { get; set; } = false;

    public ReactiveCommand<Unit, bool> Show { get; }

    public Interaction<Unit, bool> OnShowDialog { get; } = new();

    public Interaction<Unit, Unit> OnFinished { get; } = new();

    public ProgressDialogViewModel()
    {
        Show = ReactiveCommand.CreateFromTask(DoShow);
    }

    private async Task<bool> DoShow()
    {
        return await OnShowDialog.Handle(Unit.Default);
    }
}