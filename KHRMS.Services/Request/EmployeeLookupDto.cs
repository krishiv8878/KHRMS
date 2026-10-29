namespace KHRMS.Services.Request
{
    /// <summary>
    /// Lightweight, safe DTO for dropdowns, search pools, pickers, and filter lists.
    /// Excludes all sensitive PII (addresses, college grades, passport numbers, emergency contacts, banking data).
    /// </summary>
    public class EmployeeLookupDto
    {
        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName ?? ""} {LastName ?? ""}".Trim();
        public string? Designation { get; set; }
        public int? DesignationId { get; set; }
        public string? ProfileImage { get; set; }
        public string? EmailAddress { get; set; }
        public string? Department { get; set; }
        public long? ManagerId { get; set; }
        public bool IsActive { get; set; }
    }
}
