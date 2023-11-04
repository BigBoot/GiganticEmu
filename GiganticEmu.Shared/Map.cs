using System;
using System.Collections.Generic;
using System.Linq;
using Refit;

namespace GiganticEmu.Shared;

public record Map
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;

    private static IDictionary<string, Map> MAPS_CORE = new List<Map> {
            new Map { Id = "lv_canyon",        Name = "Ghost Reef" },
            new Map { Id = "lv_mistforge",     Name = "Sanctum Falls" },
            new Map { Id = "lv_valley",        Name = "Sirens Strand" },
            new Map { Id = "lv_wizardwoods",   Name = "Ember Grove (Unfinished/Prototype)" },
            new Map { Id = "lv_canyonnight",   Name = "Ghost Reef Night (Unfinished/Prototype)" },
            new Map { Id = "lv_skycityv2",     Name = "Sky City V2 (Unfinished/Prototype)" },
            new Map { Id = "lv_skytuga",       Name = "Sky Tuga (Unfinished/Prototype)" },
    }.ToDictionary(x => x.Id);

    private static IDictionary<string, Map> MAPS_M202 = new List<Map> {
            new Map { Id = "lv_canyon",        Name = "[Clash] Ghost Reef" },
            new Map { Id = "lv_mistforge",     Name = "[Clash] Sanctum Falls" },
            new Map { Id = "lv_valley",        Name = "[Clash] Sirens Strand" },
            new Map { Id = "lv_wizardwoods",   Name = "[Clash] Ember Grove" },
            new Map { Id = "lv_skycityv2",     Name = "[Clash] Sky City V2" },
            new Map { Id = "lv_modcity",       Name = "[Clash] Sky Tuga" },

            new Map { Id = "rs_canyon2",        Name = "[Rush] Ghost Reef" },
            new Map { Id = "rs_mistforge2",     Name = "[Rush] Sanctum Falls" },
            new Map { Id = "rs_valley3",        Name = "[Rush] Sirens Strand" },
            new Map { Id = "rs_wizardwoods2",   Name = "[Rush] Ember Grove" },
            new Map { Id = "rs_skycity1",       Name = "[Rush] Sky City" },
            new Map { Id = "rs_modcity1",       Name = "[Rush] Mod City" },
        }.ToDictionary(x => x.Id);

    public static IDictionary<string, Map> GetMaps(int build)
    {
        return build switch
        {
            >= GameUtils.BUILD_THROWBACK_EVENT => MAPS_M202,
            _ => MAPS_CORE,
        };
    }
}