using System;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Moq;
using Xunit;

namespace KHRMS.UnitTest
{
    public class StatutoryDeductionEngineTests
    {
        private readonly StatutoryDeductionEngine _engine = new();

        // =========================================================================
        // 1. PROVIDENT FUND (PF / EPF) TESTS
        // =========================================================================

        [Fact]
        public void CalculatePf_WhenRestrictedToWageCeiling_CapsEligibleWageAt15000()
        {
            var p = new PfCalculationParams
            {
                BasicSalary = 35000m,
                IncludePf = true,
                RestrictToWageCeiling = true,
                WageCeiling = 15000m,
                EmployeeRate = 12.0m,
                EmployerRate = 12.0m
            };

            var res = _engine.CalculatePf(p);

            Assert.Equal(15000m, res.EligiblePfWage);
            Assert.Equal(1800.00m, res.EmployeeContribution);
            Assert.Equal(1800.00m, res.EmployerContribution);
            Assert.Equal(3600.00m, res.TotalPfContribution);
        }

        [Fact]
        public void CalculatePf_WhenUnconstrained_ComputesOnFullBasic()
        {
            var p = new PfCalculationParams
            {
                BasicSalary = 35000m,
                IncludePf = true,
                RestrictToWageCeiling = false,
                EmployeeRate = 12.0m,
                EmployerRate = 12.0m
            };

            var res = _engine.CalculatePf(p);

            Assert.Equal(35000m, res.EligiblePfWage);
            Assert.Equal(4200.00m, res.EmployeeContribution);
            Assert.Equal(4200.00m, res.EmployerContribution);
            Assert.Equal(8400.00m, res.TotalPfContribution);
        }

        [Fact]
        public void CalculatePf_WhenBasicBelowCeiling_ComputesOnActualBasic()
        {
            var p = new PfCalculationParams
            {
                BasicSalary = 12000m,
                IncludePf = true,
                RestrictToWageCeiling = true,
                WageCeiling = 15000m,
                EmployeeRate = 12.0m
            };

            var res = _engine.CalculatePf(p);

            Assert.Equal(12000m, res.EligiblePfWage);
            Assert.Equal(1440.00m, res.EmployeeContribution);
            Assert.Equal(1440.00m, res.EmployerContribution);
        }

        [Fact]
        public void CalculatePf_WhenExcluded_ReturnsZero()
        {
            var p = new PfCalculationParams
            {
                BasicSalary = 25000m,
                IncludePf = false
            };

            var res = _engine.CalculatePf(p);

            Assert.Equal(0m, res.EmployeeContribution);
            Assert.Equal(0m, res.EmployerContribution);
        }

        // =========================================================================
        // 2. EMPLOYEE STATE INSURANCE (ESIC) TESTS
        // =========================================================================

        [Fact]
        public void CalculateEsic_WhenGrossWithinThreshold_ReturnsCorrectContributions()
        {
            var p = new EsicCalculationParams
            {
                GrossWage = 18000m,
                IncludeEsic = true,
                WageCeiling = 21000m,
                EmployeeRate = 0.75m,
                EmployerRate = 3.25m
            };

            var res = _engine.CalculateEsic(p);

            Assert.True(res.IsEligible);
            Assert.Equal(135.00m, res.EmployeeContribution); // 18000 * 0.0075 = 135
            Assert.Equal(585.00m, res.EmployerContribution); // 18000 * 0.0325 = 585
            Assert.Equal(720.00m, res.TotalEsicContribution);
        }

        [Fact]
        public void CalculateEsic_WhenGrossExceedsThreshold_ReturnsIneligibleZero()
        {
            var p = new EsicCalculationParams
            {
                GrossWage = 25000m,
                IncludeEsic = true,
                WageCeiling = 21000m
            };

            var res = _engine.CalculateEsic(p);

            Assert.False(res.IsEligible);
            Assert.Equal(0m, res.EmployeeContribution);
            Assert.Equal(0m, res.EmployerContribution);
        }

        // =========================================================================
        // 3. PROFESSIONAL TAX (PT) MULTI-STATE SLAB TESTS
        // =========================================================================

        [Theory]
        [InlineData("Karnataka", 12000, 1, 0)]
        [InlineData("Karnataka", 25000, 1, 200)]
        [InlineData("Maharashtra", 5000, 1, 0)]
        [InlineData("Maharashtra", 8500, 1, 175)]
        [InlineData("Maharashtra", 30000, 1, 200)]
        [InlineData("Maharashtra", 30000, 2, 300)] // February Maharashtra adjustment
        [InlineData("Telangana", 12000, 1, 0)]
        [InlineData("Telangana", 18000, 1, 150)]
        [InlineData("Telangana", 25000, 1, 200)]
        [InlineData("Gujarat", 10000, 1, 0)]
        [InlineData("Gujarat", 25000, 1, 200)]
        public void CalculateProfessionalTax_AppliesStateSpecificSlabsAndMonthRules(
            string state, decimal monthlyGross, int month, decimal expectedTax)
        {
            var p = new PtCalculationParams
            {
                MonthlyGross = monthlyGross,
                State = state,
                Month = month
            };

            var res = _engine.CalculateProfessionalTax(p);

            Assert.Equal(expectedTax, res.TaxAmount);
        }

        // =========================================================================
        // 4. INCOME TAX TDS SLABS (NEW & OLD REGIME) TESTS
        // =========================================================================

        [Fact]
        public void CalculateTds_NewRegime_WithSection87ARebate_ReturnsZeroTax()
        {
            // Annual Gross 7,00,000 - 50,000 standard deduction = 6,50,000 (below 7L threshold)
            var p = new TdsCalculationParams
            {
                AnnualGross = 700000m,
                TaxRegime = "New",
                StandardDeduction = 50000m
            };

            var res = _engine.CalculateTds(p);

            Assert.Equal(650000m, res.TaxableIncome);
            Assert.Equal(0m, res.AnnualTax);
            Assert.Equal(0m, res.MonthlyTds);
        }

        [Fact]
        public void CalculateTds_NewRegime_HighEarner_ComputesSlabsAndCess()
        {
            // Annual Gross 12,50,000 - 50,000 = 12,00,000
            // Tax: 3L@0% + 3L@5%(15k) + 3L@10%(30k) + 3L@15%(45k) = 90,000
            // Cess: 90,000 * 4% = 3,600 -> Total 93,600 -> Monthly: 7,800
            var p = new TdsCalculationParams
            {
                AnnualGross = 1250000m,
                TaxRegime = "New",
                StandardDeduction = 50000m
            };

            var res = _engine.CalculateTds(p);

            Assert.Equal(1200000m, res.TaxableIncome);
            Assert.Equal(93600m, res.AnnualTax);
            Assert.Equal(7800m, res.MonthlyTds);
        }

        // =========================================================================
        // 5. INTEGRATION TEST: PAYROLL SERVICE USES STATUTORY ENGINE
        // =========================================================================

        [Fact]
        public void PayrollService_CalculateCtcBreakdown_UsesConfigurableEngine()
        {
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockUserContext = new Mock<IUserContextService>();

            var service = new PayrollService(mockUnitOfWork.Object, mockUserContext.Object, _engine);

            // Request with Maharashtra in February and uncapped PF
            var req = new SalaryCalculationRequest
            {
                AnnualCtc = 600000m, // 50,000/mo
                BasicPercentage = 50m, // 25,000 Basic
                HraPercentage = 40m,   // 10,000 HRA
                IncludePf = true,
                RestrictPfToWageCeiling = false, // uncapped PF: 25,000 * 12% = 3,000
                IncludeEsic = true,              // Gross > 21k so ESI will be 0
                ProfessionalTaxState = "Maharashtra",
                MonthForPtCalculation = 2,       // February -> 300
                TaxRegime = "New"
            };

            var breakdown = service.CalculateCtcBreakdown(req);

            Assert.Equal(50000m, breakdown.MonthlyCtc);
            Assert.Equal(25000m, breakdown.BasicSalary);
            Assert.Equal(10000m, breakdown.Hra);
            Assert.Equal(3000m, breakdown.EpfEmployee);
            Assert.Equal(3000m, breakdown.EpfEmployer);
            Assert.Equal(0m, breakdown.EsicEmployee); // Gross > 21k
            Assert.Equal(300m, breakdown.ProfessionalTax); // February Maharashtra
        }
    }
}
