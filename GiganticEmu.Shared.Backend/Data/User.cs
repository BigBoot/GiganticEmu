using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GiganticEmu.Shared.Backend
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        public int Rank { get; set; } = 69;

        [Required]
        public string SavedLoadouts { get; set; } = "";

        [Required]
        public string ProfileSettings { get; set; } = "";

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MotigaId { get; set; }

        public string? AuthToken { get; set; } = null;

        public DateTimeOffset? AuthTokenExpires { get; set; } = null;

    }
}
