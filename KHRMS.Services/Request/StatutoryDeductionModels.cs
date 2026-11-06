using System;

namespace KHRMS.Services.Request
{
    public class PfCalculationParams
    {
        public decimal BasicSalary { get; set; }
        public bool IncludePf { get; set; } = true;
        public bool RestrictToWageCeiling { get; set; } = true;
        public decimal WageCeiling { get; set; } = 15000m;
        public decimal EmployeeRate { get; set; } = 12.0m;
        public decimal EmployerRate { get; set; } = 12.0m;
    }

    public class PfDeductionResult
    {
        public decimal EligiblePfWage { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal TotalPfContribution => EmployeeContribution + EmployerContribution;
    }

    public class EsicCalculationParams
    {
        public decimal GrossWage { get; set; }
        public bool IncludeEsic { get; set; } = true;
        public decimal WageCeiling { get; set; } = 21000m;
        public decimal EmployeeRate { get; set; } = 0.75m;
        public decimal EmployerRate { get; set; } = 3.25m;
    }

    public class EsicDeductionResult
    {
        public bool IsEligible { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal TotalEsicContribution => EmployeeContribution + EmployerContribution;
    }

    public class PtCalculationParams
    {
        public decimal MonthlyGross { get; set; }
        public string? State { get; set; } = "Karnataka";
        public int Month { get; set; } = 1; // 1 - 12
    }

    public class ProfessionalTaxResult
    {
        public decimal TaxAmount { get; set; }
        public string State { get; set; } = "Karnataka";
        public string SlabDescription { get; set; } = string.Empty;
    }

    public class TdsCalculationParams
    {
        public decimal AnnualGross { get; set; }
        public string TaxRegime { get; set; } = "New"; // "New" or "Old"
        public decimal StandardDeduction { get; set; } = 50000m;
        public decimal Section80CDeductions { get; set; } = 0m;
        public decimal Section80DDeductions { get; set; } = 0m;
        public decimal OtherExemptions { get; set; } = 0m;
    }

    public class TdsCalculationResult
    {
        public decimal TaxableIncome { get; set; }
        public decimal AnnualTax { get; set; }
        public decimal MonthlyTds { get; set; }
        public decimal CessAmount { get; set; }
        public string Regime { get; set; } = "New";
    }
}
