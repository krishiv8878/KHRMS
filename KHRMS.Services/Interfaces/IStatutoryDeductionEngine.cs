using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IStatutoryDeductionEngine
    {
        PfDeductionResult CalculatePf(PfCalculationParams parameters);
        EsicDeductionResult CalculateEsic(EsicCalculationParams parameters);
        ProfessionalTaxResult CalculateProfessionalTax(PtCalculationParams parameters);
        TdsCalculationResult CalculateTds(TdsCalculationParams parameters);
    }
}
