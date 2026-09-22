using System;
using System.Collections.Generic;

namespace KHRMS.Services.Request
{
    public class CandidateOnboardRequest
    {
        public long CandidateId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int DesignationId { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public long? ManagerId { get; set; }
        public string? ShiftId { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public string? Gender { get; set; }
        public long? RoleId { get; set; }
        public string? RoleName { get; set; }
        public long AnnualCtc { get; set; }
        public long BasicSalary { get; set; }
        public long Hra { get; set; }
        public long SpecialAllowance { get; set; }
        public long PfDeduction { get; set; }
        public long NetMonthlySalary { get; set; }
        public string? TemporaryPassword { get; set; }
        public bool SendCredentialsEmail { get; set; } = true;
        public string? ClientUrl { get; set; }
    }

    public class CandidateOnboardResponse
    {
        public long EmployeeId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool EmailSent { get; set; }
    }
}
