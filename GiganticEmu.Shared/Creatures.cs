using System.Collections.Generic;
using System.Linq;

namespace GiganticEmu.Shared;

public record Creature
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Baby { get; init; } = default!;
    public string Adult { get; init; } = default!;
    public string Family { get; init; } = default!;

    public static IDictionary<string, Creature> ALL_CREATURES = new List<Creature> {
            new Creature { Id = "cerb",             Name = "Cerberus",          Baby = "CerberusBaby",          Adult = "CerberusAdult",    Family = "cerb" },
            new Creature { Id = "cerb_shadow",      Name = "Shadow Cerberus",   Baby = "CerberusBaby_Shadow",   Adult = "CerberusShadow",   Family = "cerb" },
            new Creature { Id = "cerb_stone",       Name = "Stone Cerberus",    Baby = "CerberusBaby_Tough",    Adult = "CerberusTough",    Family = "cerb" },
            new Creature { Id = "cyclops",          Name = "Mountain Cyclops",  Baby = "CyclopsBaby",           Adult = "CyclopsAdult",     Family = "cyclops" },
            new Creature { Id = "cyclops_yeti",     Name = "Yeti Cyclops",      Baby = "CyclopsBaby_Frost",     Adult = "CyclopsFrost",     Family = "cyclops" },
            new Creature { Id = "cyclops_riftborn", Name = "Riftborn Cyclops",  Baby = "CyclopsBaby_Magic",     Adult = "CyclopsMagic",     Family = "cyclops" },
            new Creature { Id = "bloomer",          Name = "Summer Bloomer",    Baby = "EntBaby",               Adult = "EntAdult",         Family = "bloomer" },
            new Creature { Id = "bloomer_winter",   Name = "Winter Bloomer",    Baby = "EntBaby_Winter",        Adult = "EntWinter",        Family = "bloomer" },
            new Creature { Id = "bloomer_spring",   Name = "Spring Bloomer",    Baby = "EntBaby_Spring",        Adult = "EntSpring",        Family = "bloomer" },
            new Creature { Id = "bloomer_autumn",   Name = "Autumn Bloomer",    Baby = "EntBaby_Fall",          Adult = "EntFall",          Family = "bloomer" },
            new Creature { Id = "dragon",           Name = "Fire Dragon",       Baby = "DragonBaby",            Adult = "DragonAdult",      Family = "dragon" },
            new Creature { Id = "dragon_storm",     Name = "Storm Dragon",      Baby = "DragonBaby_Storm",      Adult = "DragonStorm",      Family = "dragon" },
            new Creature { Id = "obelisk",          Name = "Ancient Obelisk",   Baby = "ObeliskBaby",           Adult = "ObeliskAdult",     Family = "obelisk" },
            new Creature { Id = "infernal",         Name = "Crimson Infernal",  Baby = "DemonBaby",             Adult = "DemonAdult",       Family = "infernal" },
            new Creature { Id = "infernal_void",    Name = "Void Infernal",     Baby = "DemonBaby",             Adult = "DemonVoid",        Family = "infernal" },
            new Creature { Id = "infernal_savage",  Name = "Savage Infernal",   Baby = "DemonBaby",             Adult = "DemonOni",         Family = "infernal" },
        }.ToDictionary(x => x.Id);
}
