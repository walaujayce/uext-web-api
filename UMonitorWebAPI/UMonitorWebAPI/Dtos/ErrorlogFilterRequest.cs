namespace UMonitorWebAPI.Dtos
{
    public class ErrorlogFilterRequest
    {
        public string Deviceid { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
