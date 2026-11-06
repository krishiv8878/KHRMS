using System;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public class StatutoryDeductionEngine : IStatutoryDeductionEngine
    {
        public PfDeductionResult CalculatePf(PfCalculationParams parameters)
        {
            if (parameters == null || !parameters.IncludePf || parameters.BasicSalary <= 0)
            {
                return new PfDeductionResult
                {
                    EligiblePfWage = 0,
                    EmployeeContribution = 0,
                    EmployerContribution = 0
                };
            }

            decimal pfWage = parameters.RestrictToWageCeiling && parameters.WageCeiling > 0
                ? Math.Min(parameters.BasicSalary, parameters.WageCeiling)
                : parameters.BasicSalary;

            decimal employeeRate = parameters.EmployeeRate > 0 ? parameters.EmployeeRate : 12.0m;
            decimal employerRate = parameters.EmployerRate > 0 ? parameters.EmployerRate : 12.0m;

            decimal employeeContribution = Math.Round(pfWage * (employeeRate / 100m), 2);
            decimal employerContribution = Math.Round(pfWage * (employerRate / 100m), 2);

            return new PfDeductionResult
            {
                EligiblePfWage = pfWage,
                EmployeeContribution = employeeContribution,
                EmployerContribution = employerContribution
            };
        }

        public EsicDeductionResult CalculateEsic(EsicCalculationParams parameters)
        {
            if (parameters == null || !parameters.IncludeEsic || parameters.GrossWage <= 0)
            {
                return new EsicDeductionResult { IsEligible = false, EmployeeContribution = 0, EmployerContribution = 0 };
            }

            decimal ceiling = parameters.WageCeiling > 0 ? parameters.WageCeiling : 21000m;

            // ESIC applies only if gross wages are within the statutory wage threshold (INR 21,000)
            if (parameters.GrossWage > ceiling)
            {
                return new EsicDeductionResult
                {
                    IsEligible = false,
                    EmployeeContribution = 0,
                    EmployerContribution = 0
                };
            }

            decimal employeeRate = parameters.EmployeeRate > 0 ? parameters.EmployeeRate : 0.75m;
            decimal employerRate = parameters.EmployerRate > 0 ? parameters.EmployerRate : 3.25m;

            decimal employeeContribution = Math.Round(parameters.GrossWage * (employeeRate / 100m), 2);
            decimal employerContribution = Math.Round(parameters.GrossWage * (employerRate / 100m), 2);

            return new EsicDeductionResult
            {
                IsEligible = true,
                EmployeeContribution = employeeContribution,
                EmployerContribution = employerContribution
            };
        }

        public ProfessionalTaxResult CalculateProfessionalTax(PtCalculationParams parameters)
        {
            if (parameters == null || parameters.MonthlyGross <= 0)
            {
                return new ProfessionalTaxResult { TaxAmount = 0, State = parameters?.State ?? "Default", SlabDescription = "Gross <= 0" };
            }

            decimal gross = parameters.MonthlyGross;
            string state = (parameters.State ?? "Karnataka").Trim().ToLowerInvariant();
            int month = parameters.Month >= 1 && parameters.Month <= 12 ? parameters.Month : 1;

            decimal tax = 0;
            string slabDesc = string.Empty;

            switch (state)
            {
                case "karnataka":
                case "ka":
                    if (gross > 15000m)
                    {
                        tax = 200m;
                        slabDesc = "Karnataka: Gross > ₹15,000 (₹200/mo)";
                    }
                    else
                    {
                        tax = 0m;
                        slabDesc = "Karnataka: Gross <= ₹15,000 (Exempt)";
                    }
                    break;

                case "maharashtra":
                case "mh":
                    if (gross > 10000m)
                    {
                        // Maharashtra standard: ₹200 for 11 months, ₹300 in February to make annual ₹2,500
                        tax = month == 2 ? 300m : 200m;
                        slabDesc = month == 2
                            ? "Maharashtra: Gross > ₹10,000 (February adjustment ₹300)"
                            : "Maharashtra: Gross > ₹10,000 (₹200/mo)";
                    }
                    else if (gross > 7500m)
                    {
                        tax = 175m;
                        slabDesc = "Maharashtra: Gross ₹7,501 - ₹10,000 (₹175/mo)";
                    }
                    else
                    {
                        tax = 0m;
                        slabDesc = "Maharashtra: Gross <= ₹7,500 (Exempt)";
                    }
                    break;

                case "telangana":
                case "ts":
                case "andhra pradesh":
                case "ap":
                    if (gross > 20000m)
                    {
                        tax = 200m;
                        slabDesc = "Telangana/AP: Gross > ₹20,000 (₹200/mo)";
                    }
                    else if (gross > 15000m)
                    {
                        tax = 150m;
                        slabDesc = "Telangana/AP: Gross ₹15,001 - ₹20,000 (₹150/mo)";
                    }
                    else
                    {
                        tax = 0m;
                        slabDesc = "Telangana/AP: Gross <= ₹15,000 (Exempt)";
                    }
                    break;

                case "gujarat":
                case "gj":
                    if (gross > 12000m)
                    {
                        tax = 200m;
                        slabDesc = "Gujarat: Gross > ₹12,000 (₹200/mo)";
                    }
                    else
                    {
                        tax = 0m;
                        slabDesc = "Gujarat: Gross <= ₹12,000 (Exempt)";
                    }
                    break;

                case "tamil nadu":
                case "tn":
                    if (gross > 12500m)
                    {
                        tax = 208m;
                        slabDesc = "Tamil Nadu: Gross > ₹12,500 (₹208/mo)";
                    }
                    else if (gross > 10000m)
                    {
                        tax = 150m;
                        slabDesc = "Tamil Nadu: Gross ₹10,001 - ₹12,500 (₹150/mo)";
                    }
                    else if (gross > 7500m)
                    {
                        tax = 100m;
                        slabDesc = "Tamil Nadu: Gross ₹7,501 - ₹10,000 (₹100/mo)";
                    }
                    else
                    {
                        tax = 0m;
                        slabDesc = "Tamil Nadu: Gross <= ₹7,500 (Exempt)";
                    }
                    break;

                default:
                    // Default Tiered Slab
                    if (gross > 15000m)
                    {
                        tax = 200m;
                        slabDesc = "Default: Gross > ₹15,000 (₹200/mo)";
                    }
                    else if (gross > 10000m)
                    {
                        tax = 150m;
                        slabDesc = "Default: Gross ₹10,001 - ₹15,000 (₹150/mo)";
                    }
                    else
                    {
                        tax = 0m;
                        slabDesc = "Default: Gross <= ₹10,000 (Exempt)";
                    }
                    break;
            }

            return new ProfessionalTaxResult
            {
                TaxAmount = tax,
                State = parameters.State ?? "Default",
                SlabDescription = slabDesc
            };
        }

        public TdsCalculationResult CalculateTds(TdsCalculationParams parameters)
        {
            if (parameters == null || parameters.AnnualGross <= 0)
            {
                return new TdsCalculationResult { TaxableIncome = 0, AnnualTax = 0, MonthlyTds = 0, CessAmount = 0, Regime = parameters?.TaxRegime ?? "New" };
            }

            decimal standardDeduction = parameters.StandardDeduction > 0 ? parameters.StandardDeduction : 50000m;
            decimal totalDeductions = standardDeduction + parameters.Section80CDeductions + parameters.Section80DDeductions + parameters.OtherExemptions;
            decimal taxableIncome = Math.Max(0, parameters.AnnualGross - totalDeductions);

            decimal baseTax = 0;
            string regime = parameters.TaxRegime?.Equals("Old", StringComparison.OrdinalIgnoreCase) == true ? "Old" : "New";

            if (regime == "Old")
            {
                // Section 87A rebate for taxable income <= 5,00,000
                if (taxableIncome > 500000m)
                {
                    if (taxableIncome > 1000000m)
                    {
                        baseTax = 112500m + ((taxableIncome - 1000000m) * 0.30m);
                    }
                    else
                    {
                        baseTax = 12500m + ((taxableIncome - 500000m) * 0.20m);
                    }
                }
                else if (taxableIncome > 250000m)
                {
                    // Full rebate under 87A
                    baseTax = 0;
                }
            }
            else
            {
                // New Tax Regime: Section 87A rebate for taxable income <= 7,00,000
                if (taxableIncome > 700000m)
                {
                    if (taxableIncome > 1500000m)
                        baseTax = 150000m + ((taxableIncome - 1500000m) * 0.30m);
                    else if (taxableIncome > 1200000m)
                        baseTax = 90000m + ((taxableIncome - 1200000m) * 0.20m);
                    else if (taxableIncome > 900000m)
                        baseTax = 45000m + ((taxableIncome - 900000m) * 0.15m);
                    else if (taxableIncome > 600000m)
                        baseTax = 15000m + ((taxableIncome - 600000m) * 0.10m);
                    else if (taxableIncome > 300000m)
                        baseTax = (taxableIncome - 300000m) * 0.05m;
                }
                else
                {
                    baseTax = 0;
                }
            }

            // 4% Health & Education cess
            decimal cess = Math.Round(baseTax * 0.04m, 2);
            decimal totalAnnualTax = baseTax + cess;
            decimal monthlyTds = Math.Round(totalAnnualTax / 12m, 2);

            return new TdsCalculationResult
            {
                TaxableIncome = taxableIncome,
                AnnualTax = totalAnnualTax,
                MonthlyTds = monthlyTds,
                CessAmount = cess,
                Regime = regime
            };
        }
    }
}
