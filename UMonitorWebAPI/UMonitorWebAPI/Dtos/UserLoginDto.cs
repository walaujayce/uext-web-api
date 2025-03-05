namespace UMonitorWebAPI.Dtos
{
    public class UserLoginDto
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        public string Userid { get; set; } = null!;

        /// <summary>
        /// 使用者密碼
        /// </summary>
        public string Password { get; set; } = null!;
    }
}
