using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class UserLogin : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("UserRegistration")]
        public long UserId { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "LastLoginDate is required")]
        public DateTime LastLoginDate { get; set; }

        [Required(ErrorMessage = "isPasswordRequired is required")]
        public bool? IsResetPasswordRequired { get; set; }
    }
}
