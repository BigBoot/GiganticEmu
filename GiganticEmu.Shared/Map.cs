using System.Collections.Generic;
using System.Linq;

namespace GiganticEmu.Shared;

public record Map
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;

    public static IDictionary<string, Map> ALL_MAPS = new List<Map> {
            new Map { Id = "lv_canyon",        Name = "Ghost Reef" },
            new Map { Id = "lv_mistforge",     Name = "Sanctum Falls" },
            new Map { Id = "lv_valley",        Name = "Sirens Strand" },
            new Map { Id = "lv_wizardwoods",   Name = "Ember Grove (Unfinished/Prototype)" },
            new Map { Id = "lv_canyonnight",   Name = "Ghost Reef (Unfinished/Prototype)" },
            new Map { Id = "lv_skycityv2",     Name = "Sky City V2 (Unfinished/Prototype)" },
            new Map { Id = "lv_skytuga",       Name = "Sky Tuga (Unfinished/Prototype)" },
        }.ToDictionary(x => x.Id);
}
