using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiganticEmu.Shared.Backend;

public class ReportToken : EntityBase
{
    public string Token { get; set; } = default!;
    public DateTimeOffset ValidUntil { get; set; } = DateTimeOffset.Now.AddHours(3);
}