using System;
using System.Collections.Generic;

namespace UMonitorWebAPI.Models;

public partial class Errorlog
{
    public Guid Guid { get; set; }

    public string? Deviceid { get; set; }

    public string Logtype { get; set; } = null!;

    public string Log { get; set; } = null!;

    public DateTime Logtime { get; set; }

    public bool? CheckStatus { get; set; }
}
