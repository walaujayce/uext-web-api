using System;
using System.Collections.Generic;

namespace UMonitorWebAPI.Models;

public partial class Patient
{
    public string Patientname { get; set; } = null!;

    public string Patientid { get; set; } = null!;

    public int Sex { get; set; }

    public DateTime Birthday { get; set; }

    public double Height { get; set; }

    public double Weight { get; set; }

    public string? Deviceid { get; set; }

    public virtual Device? Device { get; set; }
}
