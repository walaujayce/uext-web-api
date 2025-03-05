namespace UMonitorWebAPI.Dtos
{
    public class UserPutDto
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// 使用者密碼
        /// </summary>
        public string Password { get; set; } = null!;

        /// <summary>
        /// 使用者Email
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// 0 = Administrator(A);1 = Engineer(E);2 = User(U)
        /// </summary>
        public int Role { get; set; }
    }
}
