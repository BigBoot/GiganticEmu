using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class InputDialogViewModel : ReactiveObject, IDialogViewModel<string>
{
    [Reactive]
    public string Title { get; set; } = "Title";

    [Reactive]
    public string? Input { get; set; } = "";

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

    public ReactiveCommand<Unit, string?> Show { get; }

    public Interaction<Unit, string?> OnShowDialog { get; } = new();

    public InputDialogViewModel()
    {
        Show = ReactiveCommand.CreateFromTask(DoShow);
    }

    private async Task<string?> DoShow()
    {
        return await OnShowDialog.Handle(Unit.Default);
    }
}