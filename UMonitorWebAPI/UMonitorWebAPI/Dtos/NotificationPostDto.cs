using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Dtos
{
    public class NotificationPostDto
    {
        public string Deviceid { get; set; } = null!;

        public string? NotifyBody { get; set; }

        public bool? CheckStatus { get; set; }

        public DateTime? PunchTime { get; set; }
    }
}
