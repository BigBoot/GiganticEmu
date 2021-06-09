using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiganticEmu.Shared.Backend
{
    public class PlayerInfo : EntityBase
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string SavedLoadouts { get; set; } = "";

        [Required]
        public string ProfileSettings { get; set; } = "";

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MotigaId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
