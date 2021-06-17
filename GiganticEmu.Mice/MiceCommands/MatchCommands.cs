using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public static class MatchCommands
{
    [MiceCommand("match.me")]
    public static async Task<object> JoinQueue(dynamic payload, MiceClient client)
    {
        using var db = client.CreateDbContext();
        var sessionId = (await db.Users.SingleAsync(user => user.Id == client.UserId)).SessionId;
        var group = await db.Users.Where(user => user.SessionId == sessionId).ToListAsync();
        foreach (var user in group)
        {
            user.InQueue = true;
        }
        await db.SaveChangesAsync();

        return new { };
    }

    [MiceCommand("stop.matching")]
    public static async Task<object> LeaveQueue(dynamic payload, MiceClient client)
    {
        using var db = client.CreateDbContext();
        var sessionId = (await db.Users.SingleAsync(user => user.Id == client.UserId)).SessionId;
        var group = await db.Users.Where(user => user.SessionId == sessionId).ToListAsync();
        foreach (var user in group)
        {
            user.InQueue = false;
        }
        await db.SaveChangesAsync();

        return new { };
    }

}