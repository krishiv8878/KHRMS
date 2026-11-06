using System;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class InviteEmployeeRequest
    {
        public long? EmployeeId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MobileNumber { get; set; }
        public string? ClientUrl { get; set; }
    }

    public class InviteEmployeeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? InvitationToken { get; set; }
        public string? InvitationUrl { get; set; }
        public bool EmailSent { get; set; }
    }

    public class AcceptInvitationRequest
    {
        [Required(ErrorMessage = "Invitation token is required")]
        public string Token { get; set; } = string.Empty;
            
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Address { get; set; }
    }

    public class VerifyInvitationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public long? EmployeeId { get; set; }
    }
}
