using System;
using System.Linq;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using Microsoft.EntityFrameworkCore;

public static class PlayerCommands
{
    [MiceCommand("player.getservertime")]
    public static async Task<object?> GetServerTime(dynamic payload, MiceClient client) => new
    {
        datetime = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss")
    };

    [MiceCommand("player.setinfo")]
    public static async Task<object?> SetInfo(dynamic payload, MiceClient client)
    {
        var user = await client.Database.Users.SingleAsync(user => user.Id == client.UserId);

        user.ProfileSettings = ((object)payload.profilesettings).ToJson();
        user.SavedLoadouts = ((object)payload.savedLoadouts).ToJson();

        await client.Database.SaveChangesAsync();

        return null;
    }

    [MiceCommand("player.getinfo")]
    public static async Task<object?> GetInfo(dynamic payload, MiceClient client)
    {
        var user = await client.Database.Users.SingleAsync(user => user.Id == client.UserId);

        return new
        {
            player = new
            {
                savedLoadouts = user.SavedLoadouts.FromJsonTo<dynamic>(),
                profilesettings = user.ProfileSettings.FromJsonTo<dynamic>(),
                instanceid = "instanceStr",
                penaltyHistory = new { },
                moid = user.MotigaId,
                inventory = new[] { Heroes.RESOURCE_IDS, Skins.RESOURCE_IDS, Creatures.RESOURCE_IDS }
                    .SelectMany(ids => ids.Select(id => new { quantity = 1, resource_id = id, origins = new[] { "owned", "flagged" } })),
                preview_matches_left = new
                {
                    keyStr = "valueStr"
                }
            }
        };
    }

    [MiceCommand("player.progressionget")]
    public static async Task<object> ProgressionGet(dynamic payload, MiceClient client)
    {
        var user = await client.Database.Users.SingleAsync(user => user.Id == client.UserId);

        return new
        {
            progression = new
            {
                account_rank = new
                {
                    list = new[] {
                    new {
                        current_value = "valueStr",
                        rank = user.Rank,
                        metric = "metricStr",
                        teir = "teirStr",
                        name = "nameStr",
                        date = "dateStr",
                        current_rank = new {},
                        target = new[] {
                            new { keyStr = "valueStr" },
                        },
                        rewards = "rewardStr",
                        next_rank = new
                        {
                            keyStr = "valueStr",
                        }
                    }
                },
                },
                badge = new
                {
                    list = new[] {
                        new {
                        rank = "rankStr",
                        badge = "badgeStr",
                    }},
                    medal = new
                    {
                        keyStr = "valueStr"
                    },
                },
            },
            signInDesc = new
            {
                keyStr = "valueStr"
            }
        };
    }

    [MiceCommand("player.getgamestatus")]
    public static async Task<object> GetGameStatus(dynamic payload, MiceClient client) => new
    {
        end_date = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
        state = "stateStr",
        countdown = false,
    };
}