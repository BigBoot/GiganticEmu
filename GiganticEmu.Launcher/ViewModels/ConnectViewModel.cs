using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace GiganticEmu.Launcher;

public class ConnectViewModel : ReactiveObject
{

    [Reactive]
    public string UserName { get; set; } = "";

    [Reactive]
    public string Host { get; set; } = "";

    public ReactiveCommand<Unit, Unit> Connect { get; }

    public Interaction<DialogContent, Unit> OnError { get; } = new();

    public Interaction<DialogContent, bool> OnConfirm { get; } = new();

    public ConnectViewModel()
    {
        Connect = ReactiveCommand.CreateFromTask(DoConnect);

        var settings = Locator.Current.RequireService<Settings>();

        Host = settings.LastHost.Value;
        UserName = settings.OfflineName.Value;

        this.WhenAnyValue(x => x.Host)
            .Do(x => { settings.LastHost.OnNext(x); })
            .Subscribe();

        this.WhenAnyValue(x => x.UserName)
            .Do(x => { settings.OfflineName.OnNext(x); })
            .Subscribe();
    }

    private async Task DoConnect()
    {
        var settings = Locator.Current.RequireService<Settings>();
        var launcher = Locator.Current.RequireService<GameLauncher>();
        await settings.Save();

        launcher.StartGame(new GameLauncher.InteractionHandler
        {
            OnConfirm = async content => await OnConfirm.Handle(content),
            OnError = async content => await OnError.Handle(content),
        }, settings.OfflineName.Value, settings.OfflineName.Value, host: settings.LastHost.Value);
    }
}