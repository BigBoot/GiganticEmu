using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiganticEmu.Shared.Backend
{
    public class Progression : EntityBase
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int Rank { get; set; } = 69;

        public virtual User User { get; set; } = null!;
    }
}
