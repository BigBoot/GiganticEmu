using System;
using System.ComponentModel.DataAnnotations;

namespace GiganticEmu.Shared.Backend
{
    public abstract class EntityBase
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
