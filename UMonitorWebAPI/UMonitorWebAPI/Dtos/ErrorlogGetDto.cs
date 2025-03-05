namespace UMonitorWebAPI.Dtos
{
    public class ErrorlogGetDto
    {
        public Guid Guid { get; set; }

        public string Deviceid { get; set; } = null!;

        public int DeviceType{ get; set; }

        public string Logtype { get; set; } = null!;

        public string Log { get; set; } = null!;

        public DateTime Logtime { get; set; }

        public bool? CheckStatus { get; set; }


    }
}
