using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GiganticEmu.Shared;

public static class ExceptionUtils
{
    public static async Task LogExceptions(this Task t, ILogger logger, string? message = null, params object?[] args)
    {
        try
        {
            await t;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, message, args);
        }
    }
}