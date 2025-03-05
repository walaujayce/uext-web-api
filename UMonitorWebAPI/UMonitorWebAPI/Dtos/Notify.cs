using MimeKit.Encodings;

namespace UMonitorWebAPI.Dtos
{
    public class Notify
    {
        public Guid Id { get; set; }

        public string Macaddress { get; set; } = null!;

    }
}
