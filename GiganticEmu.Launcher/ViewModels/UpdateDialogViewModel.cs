using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static GiganticEmu.Launcher.GitHub;

namespace GiganticEmu.Launcher;

public class UpdateDialogViewModel : ReactiveObject, IDialogViewModel<UpdateDialogViewModel.Result>
{
    public enum Result { Update, Skip, Later }

    [Reactive]
    public Changelog Changelog { get; set; } = new Changelog(new List<Changelog.Version>()
    {
        new Changelog.Version(new Shared.SemVer(1, 0, 0), System.DateTime.Now, new List<Changelog.Section>(){
            new Changelog.Section("Added", new List<string> { "Cool stuff", "More cool stuff", "Even more cool stuff" }),
            new Changelog.Section("Changed", new List<string> { "Not so cool stuff", "Some other stuff" }),
            new Changelog.Section("Remove", new List<string> { "Actually bad stuff", "A bug" }),
        })
    });

    public ReactiveCommand<Unit, Result> Show { get; }

    public Interaction<Unit, Result> OnShowDialog { get; } = new();

    public UpdateDialogViewModel()
    {
        Show = ReactiveCommand.CreateFromTask(DoShow);
    }

    private async Task<Result> DoShow()
    {
        return await OnShowDialog.Handle(Unit.Default);
    }
}