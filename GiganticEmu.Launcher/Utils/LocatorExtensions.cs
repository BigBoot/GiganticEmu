using System;
using Splat;

namespace GiganticEmu.Launcher;

public static class LocatorExtensions
{
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(Type t)
            : base($"Unable to find required service of type '{t.Name}'") { }
    }

    public static T RequireService<T>(this IReadonlyDependencyResolver locator, string? contract = null)
    {
        return locator.GetService<T>(contract) ?? throw new ServiceNotFoundException(typeof(T));
    }
}