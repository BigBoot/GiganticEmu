using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GiganticEmu.Launcher;

public class AlertDialogViewModel : ReactiveObject, IDialogViewModel<string>
{
    public record Button(string Text, string? Value = null);

    public ISourceList<Button> Buttons { get; } = new SourceList<Button>();

    [Reactive]
    public string Title { get; set; } = "Title";

    [Reactive]
    public string? Text { get; set; } = "Content";

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

    public AlertDialogViewModel()
    {
        Show = ReactiveCommand.CreateFromTask(DoShow);
    }

    private async Task<string?> DoShow()
    {
        return await OnShowDialog.Handle(Unit.Default);
    }
}