using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public static class MatchCommands
{
    [MiceCommand("match.me")]
    public static async Task<object> JoinQueue(dynamic payload, MiceClient client)
    {
        var user = await client.Database.Users.SingleAsync(user => user.Id == client.UserId);
        user.InQueue = true;
        await client.Database.SaveChangesAsync();

        return new { };
    }

    [MiceCommand("stop.matching")]
    public static async Task<object> LeaveQueue(dynamic payload, MiceClient client)
    {
        var user = await client.Database.Users.SingleAsync(user => user.Id == client.UserId);
        user.InQueue = false;
        await client.Database.SaveChangesAsync();

        return new { };
    }

}