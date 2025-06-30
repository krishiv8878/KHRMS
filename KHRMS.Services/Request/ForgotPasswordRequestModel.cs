
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class ForgotPasswordRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string ClientUrl { get; set; }
    }
}
