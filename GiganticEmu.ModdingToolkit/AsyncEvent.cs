using System;
using System.Threading.Tasks;

namespace GiganticEmu.ModdingToolkit;

public class AsyncEvent<TEventArgs>
{
    private readonly TaskCompletionSource<TEventArgs> _eventArrived = new TaskCompletionSource<TEventArgs>();

    private readonly Action<EventHandler<TEventArgs>> _unsubscribe;

    public AsyncEvent(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe)
    {
        subscribe(Subscription);
        _unsubscribe = unsubscribe;
    }

    public Task<TEventArgs> Task => _eventArrived.Task;

    private EventHandler<TEventArgs> Subscription => (s, e) =>
        {
            _eventArrived.TrySetResult(e);
            _unsubscribe(Subscription);
        };
}