namespace UMonitorWebAPI.Models;

public partial class Notification
{
    public Guid Id { get; set; }

    public string Deviceid { get; set; } = null!;

    public string? NotifyBody { get; set; }

    public bool? CheckStatus { get; set; }

    public DateTime? PunchTime { get; set; }

    public DateTime? CreateDate { get; set; }
}
