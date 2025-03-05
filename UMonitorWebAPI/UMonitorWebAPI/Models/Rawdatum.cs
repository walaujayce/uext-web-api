using System;
using System.Collections.Generic;

namespace UMonitorWebAPI.Models;

public partial class Rawdatum
{
    public Guid Rawid { get; set; }

    public string Deviceid { get; set; } = null!;

    public string Data { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public string? Devicetype { get; set; }

    public int? Frameid { get; set; }
}
