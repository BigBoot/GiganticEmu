using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;

public static class PlayerCommands
{
    [MiceCommand("player.getservertime")]
    public static async Task<object?> GetServerTime(dynamic payload) => new
    {
        datetime = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss")
    };

    [MiceCommand("player.setinfo")]
    public static async Task<object?> SetInfo(dynamic payload)
    {
        await File.WriteAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "profilesettings.json"), ((object)payload.profilesettings).ToJson());
        await File.WriteAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "savedloadouts.json"), ((object)payload.savedLoadouts).ToJson());
        return null;
    }

    [MiceCommand("player.getinfo")]
    public static async Task<object?> GetInfo(dynamic payload) => new
    {
        player = new
        {
            savedLoadouts = File.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "savedloadouts.json")) ? (await File.ReadAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "profilesettings.json"))).FromJsonTo<dynamic>() : new object[] { },
            profilesettings = File.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "profilesettings.json")) ? (await File.ReadAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "profilesettings.json"))).FromJsonTo<dynamic>() : new { },
            instanceid = "instanceStr",
            penaltyHistory = new { },
            moid = 2,
            inventory = new[] { Heroes.RESOURCE_IDS, Skins.RESOURCE_IDS, Creatures.RESOURCE_IDS }
                .SelectMany(ids => ids.Select(id => new { quantity = 1, resource_id = id, origins = new[] { "owned", "flagged" } })),
            preview_matches_left = new
            {
                keyStr = "valueStr"
            }
        }
    };

    [MiceCommand("player.progressionget")]
    public static async Task<object> ProgressionGet(dynamic payload) => new
    {
        progression = new
        {
            account_rank = new
            {
                list = new[] {
                    new {
                        current_value = "valueStr",
                        rank = 69,
                        metric = "metricStr",
                        teir = "teirStr",
                        name = "nameStr",
                        date = "dateStr",
                        current_rank = new {},
                        target = new[] {
                            new { keyStr = "valueStr" },
                        },
                        rewards = "rewardStr",
                        next_rank = new {
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

    [MiceCommand("player.getgamestatus")]
    public static async Task<object> GetGameStatus(dynamic payload) => new
    {
        end_date = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
        state = "stateStr",
        countdown = false,
    };
}