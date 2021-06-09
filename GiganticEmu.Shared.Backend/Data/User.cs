using System;
using Microsoft.AspNetCore.Identity;

namespace GiganticEmu.Shared.Backend
{
    public class User : IdentityUser<Guid>
    {
        public virtual Progression Progression { get; set; } = null!;

        public virtual PlayerInfo PlayerInfo { get; set; } = null!;
    }
}
