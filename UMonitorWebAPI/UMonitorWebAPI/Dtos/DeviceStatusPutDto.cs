using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Dtos
{
    public class DeviceStatusPutDto
    {
        public int Devicetype { get; set; }

        public string Macaddress { get; set; } = null!;

        public string Ipaddress { get; set; } = null!;

        public int Devicestatus { get; set; }

        public bool Used { get; set; }

        public bool Connect { get; set; }

        public string? Actionid { get; set; }

        public int DisconnectCnt { get; set; }

        public string? RecordMode { get; set; }

        public string? Version { get; set; }

        public int Judgemethod { get; set; }

        public int Edgepar { get; set; }

        public int Edgebox { get; set; }

        public int Sitpar { get; set; }

        public int Sitbox { get; set; }

        public int HeightTh { get; set; }

        public string? ErMap { get; set; }

        public int Vmax { get; set; }

        public int Vmin { get; set; }

        public int DebTst { get; set; }

        public int DebFps { get; set; }

        public int BoxYStart { get; set; }

        public int EdgeSitPoint { get; set; }

        public int Emasize { get; set; }

        public int Emathres { get; set; }

        public int Noisethres { get; set; }

        public int Pmio { get; set; }

    }
}
