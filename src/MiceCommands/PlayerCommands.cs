using System;
using System.IO;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;

public static class PlayerCommands
{

    [MiceCommand("player.getservertime")]
    public static async Task<object> GetServerTime(dynamic payload) => new
    {
        datetime = DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss")
    };

    [MiceCommand("player.setinfo")]
    public static async Task<object> SetInfo(dynamic payload)
    {
        await File.WriteAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "profilesettings.json"), ((object)payload.profilesettings).ToJson());
        return null;
    }

    [MiceCommand("player.getinfo")]
    public static async Task<object> GetInfo(dynamic payload) => new
    {
        player = new
        {
            savedLoadouts = new[] {
                new { guardianLoadout = new[] { "EntWinter", "CyclopsAdult", "DragonAdult", "EntBaby_Winter", "CyclopsBaby", "DragonBaby" } },
                new { guardianLoadout = new[] { "EntWinter", "CyclopsAdult", "DragonAdult", "EntBaby_Winter", "CyclopsBaby", "DragonBaby" } },
                new { guardianLoadout = new[] { "EntWinter", "CyclopsAdult", "DragonAdult", "EntBaby_Winter", "CyclopsBaby", "DragonBaby" } },
            },
            profilesettings = File.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "profilesettings.json")) ? (await File.ReadAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "profilesettings.json"))).FromJsonTo<dynamic>() : new { },
            instanceid = "instanceStr",
            penaltyHistory = new { },
            moid = 2,
            inventory = new[] {
                  new { quantity = 1, resource_id = "Adept", origins = new[]{"owned", "flagged"} },
                  new { quantity = 1, resource_id = "Alchemist", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Angel", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Assault", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Aura", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Blade", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Bombard", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Despair", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Frosty", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Judo", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Machine", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Minotaur", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Planter", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Quarrel", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Rogue", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Swift", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Tank", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Warden", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Zap", origins = new[]{"owned", "flagged"}},
                  new { quantity = 1, resource_id = "Glyph", origins = new[]{"owned", "owned"}},
            },
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