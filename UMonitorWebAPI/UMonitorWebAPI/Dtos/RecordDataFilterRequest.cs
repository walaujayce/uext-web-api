namespace UMonitorWebAPI.Dtos
{
    public class RecordDataFilterRequest
    {
        public string Deviceid { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
