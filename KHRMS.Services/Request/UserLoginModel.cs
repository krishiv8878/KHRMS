
namespace KHRMS.Services.Request
{
    public class UserLoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? RoleType { get; set; }
        public string? UserName { get; set; }
        public string? Token { get; set; }
        public long? UserId { get; set; }
        public bool? ProfileCompleted { get; set; }
        public bool? IsResetPasswordRequired { get; set; }
    }
}
