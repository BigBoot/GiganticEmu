using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class MiceCommandHandler
{
    private Dictionary<string, Func<dynamic, MiceClient, Task<object>>> Handlers { get; init; }

    public MiceCommandHandler()
    {
        Handlers = Assembly.GetEntryAssembly()?
                    .GetTypes()
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes<MiceCommandAttribute>(false).Any())
                    .ToDictionary(m => m.GetCustomAttribute<MiceCommandAttribute>(false)!.Command,
                        m => (Func<dynamic, MiceClient, Task<object>>)Delegate.CreateDelegate(typeof(Func<dynamic, MiceClient, Task<object>>), null, m))
                    ?? new Dictionary<String, Func<dynamic, MiceClient, Task<object>>>();
    }

    public bool CanHandle(string cmd)
    {
        return Handlers.ContainsKey(cmd);
    }

    public object Handle(string cmd, dynamic payload, MiceClient client)
    {
        return Handlers[cmd](payload, client);
    }
}