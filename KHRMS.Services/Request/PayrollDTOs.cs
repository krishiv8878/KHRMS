namespace KHRMS.Services.Request
{
    public class SalaryCalculationRequest
    {
        public decimal AnnualCtc { get; set; }
        public decimal BasicPercentage { get; set; } = 40; // 40% of CTC
        public decimal HraPercentage { get; set; } = 50;   // 50% of Basic
        public bool IncludePf { get; set; } = true;
        public bool IncludeEsic { get; set; } = true;
        public string TaxRegime { get; set; } = "New";     // "New" or "Old"
    }

    public class SalaryCalculationResult
    {
        public decimal AnnualCtc { get; set; }
        public decimal MonthlyCtc { get; set; }
        public decimal MonthlyGross { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Hra { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal EpfEmployee { get; set; }
        public decimal EpfEmployer { get; set; }
        public decimal EsicEmployee { get; set; }
        public decimal EsicEmployer { get; set; }
        public decimal ProfessionalTax { get; set; }
        public decimal MonthlyTds { get; set; }
        public decimal TotalMonthlyDeductions { get; set; }
        public decimal MonthlyNetTakeHome { get; set; }
        public string TaxRegime { get; set; } = "New";
    }

    public class AssignSalaryRequest
    {
        public long EmployeeId { get; set; }
        public long? SalaryStructureId { get; set; }
        public decimal AnnualCtc { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? Hra { get; set; }
        public decimal? SpecialAllowance { get; set; }
        public decimal? EpfEmployee { get; set; }
        public decimal? EpfEmployer { get; set; }
        public decimal? EsicEmployee { get; set; }
        public decimal? EsicEmployer { get; set; }
        public decimal? ProfessionalTax { get; set; }
        public decimal? MonthlyTds { get; set; }
        public string TaxRegime { get; set; } = "New";
        public DateTime EffectiveFromDate { get; set; } = DateTime.UtcNow;
        public string? Remarks { get; set; }
    }

    public class CreatePayRunRequest
    {
        public int Month { get; set; } // 1 - 12
        public int Year { get; set; }
        public string? PaymentMode { get; set; } = "Bank Transfer";
        public string? Notes { get; set; }
    }

    public class UpdatePayRunStatusRequest
    {
        public long PayRunId { get; set; }
        public string Status { get; set; } = "Submitted"; // Submitted, Approved, Disbursed, Rejected
        public string? Notes { get; set; }
    }

    public class ReimbursementClaimRequest
    {
        public long EmployeeId { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime BillDate { get; set; }
        public string? BillNumber { get; set; }
        public string? MerchantName { get; set; }
        public string? Description { get; set; }
        public string? ReceiptUrl { get; set; }
    }

    public class ReviewReimbursementRequest
    {
        public long ClaimId { get; set; }
        public string Status { get; set; } = "Approved"; // Approved or Rejected
        public decimal? ApprovedAmount { get; set; }
        public string? Remarks { get; set; }
    }

    public class FullAndFinalCalculationRequest
    {
        public long EmployeeId { get; set; }
        public long? ResignationId { get; set; }
        public DateTime RelievingDate { get; set; }
        public int NoticePeriodDays { get; set; } = 30;
        public int NoticePeriodServedDays { get; set; } = 30;
        public decimal PendingSalaryDays { get; set; } = 0;
        public decimal OtherAllowances { get; set; } = 0;
        public decimal OtherDeductions { get; set; } = 0;
        public string? Remarks { get; set; }
    }

    public class PayslipDto
    {
        public long Id { get; set; }
        public long PayRunId { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string Pan { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public string PayslipNumber { get; set; } = string.Empty;
        public decimal TotalWorkingDays { get; set; }
        public decimal PresentDays { get; set; }
        public decimal LopDays { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Hra { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal Reimbursements { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal EpfDeduction { get; set; }
        public decimal EsicDeduction { get; set; }
        public decimal PtDeduction { get; set; }
        public decimal TdsDeduction { get; set; }
        public decimal LopDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class BankPayoutRecordDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
