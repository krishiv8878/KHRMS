using KHRMS.Core.Models;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface ICompanyPolicyService
    {
        Task<List<PolicyDto>> GetAllPoliciesAsync(long? currentEmployeeId = null, string? category = null, bool activeOnly = true);
        Task<PolicyDto?> GetPolicyByIdAsync(long id, long? currentEmployeeId = null);
        Task<CompanyPolicy> CreatePolicyAsync(CreateCompanyPolicyRequest request, long createdBy);
        Task<CompanyPolicy> UpdatePolicyAsync(UpdateCompanyPolicyRequest request, long updatedBy);
        Task<bool> DeletePolicyAsync(long id);
        Task<PolicyAcknowledgement> AcknowledgePolicyAsync(long policyId, int version, long employeeId, string? ipAddress);
        Task<List<PolicyAcknowledgement>> GetPolicyAcknowledgementsAsync(long policyId);
        Task<PolicyComplianceSummaryDto> GetComplianceSummaryAsync();
    }
}
