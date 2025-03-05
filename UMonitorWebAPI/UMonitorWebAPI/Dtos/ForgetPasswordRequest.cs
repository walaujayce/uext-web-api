namespace UMonitorWebAPI.Dtos
{
    public class ForgetPasswordRequest
    {
        public string Email { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
