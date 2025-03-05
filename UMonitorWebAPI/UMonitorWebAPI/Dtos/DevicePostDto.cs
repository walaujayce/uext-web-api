using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Dtos
{
    public class DevicePostDto
    {
        public int Devicetype { get; set; }

        //public string Deviceid { get; set; } = null!;

        public string Macaddress { get; set; } = null!;

        //public string Ipaddress { get; set; } = null!;

        public bool Used { get; set; } = false;

        public string? Bed { get; set; }

        public string? Floor { get; set; }

        public string? Section { get; set; }

    }
}
