using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GiganticEmu.Shared.Backend;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GiganticEmu.Web;

public class DatabaseCleanupService : IHostedService, IDisposable
{
    private readonly ILogger<DatabaseCleanupService> _logger;
    private Timer _timer;
    private ApplicationDatabase _database;

    public DatabaseCleanupService(ILogger<DatabaseCleanupService> logger, ApplicationDatabase database)
    {
        _logger = logger;
        _timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
        _database = database;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{} is starting.", nameof(DatabaseCleanupService));

        _timer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(30));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogDebug("{} cleanup starting.", nameof(DatabaseCleanupService));

        using var transaction = _database.Database.BeginTransaction();

        var users = await _database.Users.Where(user => user.AuthTokenExpires != null && user.AuthTokenExpires <= DateTimeOffset.Now).ToListAsync();
        foreach (var user in users)
        {
            user.AuthToken = null;
            user.AuthTokenExpires = null;
        }

        _database.RemoveRange(await _database.ReportTokens.Where(token => token.ValidUntil <= DateTimeOffset.Now).ToListAsync());

        await _database.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{} is stopping.", nameof(DatabaseCleanupService));

        _timer.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

}
