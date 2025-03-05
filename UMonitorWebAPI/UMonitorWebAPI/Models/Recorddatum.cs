using System;
using System.Collections.Generic;

namespace UMonitorWebAPI.Models;

public partial class Recorddatum
{
    public string Deviceid { get; set; } = null!;

    public DateTime Recordtime { get; set; }

    public string Statusid { get; set; } = null!;

    public int Duration { get; set; }

    public int? Frameid { get; set; }
}
