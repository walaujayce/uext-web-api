namespace UMonitorWebAPI.Dtos
{
    public class RawdatumFilterRequest
    {
        public string Deviceid { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

    }
}
