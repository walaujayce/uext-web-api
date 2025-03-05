using System;
using System.Collections.Generic;

namespace UMonitorWebAPI.Models;

public partial class Device
{
    public int Devicetype { get; set; }

    public string Deviceid { get; set; } = null!;

    public string Macaddress { get; set; } = null!;

    public string Ipaddress { get; set; } = "";

    public int Devicestatus { get; set; } = 0;

    public bool Used { get; set; } = false;

    public bool Connect { get; set; } = false;

    public string? Actionid { get; set; } = "01";

    public int DisconnectCnt { get; set; } = 0;

    public string? RecordMode { get; set; } = "03";

    public DateTime? Updatedat { get; set; }

    public string? Version { get; set; }

    public int Judgemethod { get; set; } = 1;

    public int Edgepar { get; set; } = 90;

    public int Edgebox { get; set; } = 40;

    public int Sitpar { get; set; } = 90;

    public int Sitbox { get; set; } = 40;

    public int HeightTh { get; set; } = 12;

    public string? ErMap { get; set; }

    public int Vmax { get; set; } = 800;

    public int Vmin { get; set; } = 200;

    public int DebTst { get; set; } = 1;

    public int DebFps { get; set; } = 1;

    public int BoxYStart { get; set; } = 7;

    public int EdgeSitPoint { get; set; } = 3;

    public int Emasize { get; set; } = 1;

    public int Emathres { get; set; } = 10;

    public int Noisethres { get; set; } = 2;

    public int Pmio { get; set; } = 100;

    public string? Bed { get; set; }

    public string? Floor { get; set; }

    public string? Section { get; set; }

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
