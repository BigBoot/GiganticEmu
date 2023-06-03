using System.Collections.Generic;
using System.Linq;

namespace GiganticEmu.Shared;

public record Guardian
{
    public int Id { get; init; } = default!;
    public string CodeName { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string House { get; init; } = default!;

    public static IList<Guardian> ALL_GUARDIANS = new List<Guardian> {
        new Guardian { Id = 0, CodeName = "griffin", Name = "Leiran", House = "Aurion"   },
        new Guardian { Id = 1, CodeName = "naga",    Name = "Grenn",  House = "Devaedra" },
    };
}
