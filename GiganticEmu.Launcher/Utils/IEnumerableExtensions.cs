using System;
using System.Collections.Generic;
using System.Linq;

namespace GiganticEmu.Launcher;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Do<T>(this IEnumerable<T> e, Action<T> action)
    {
        return e.Select(x =>
        {
            action(x);
            return x;
        });
    }
}