using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public class CompanyPolicyService : ICompanyPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyPolicyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<PolicyDto>> GetAllPoliciesAsync(long? currentEmployeeId = null, string? category = null, bool activeOnly = true)
        {
            var policies = (await _unitOfWork.CompanyPolicies.GetAll())
                .Where(p => !p.IsDeleted)
                .ToList();

            if (activeOnly)
            {
                policies = policies.Where(p => p.IsActive).ToList();
            }

            if (!string.IsNullOrEmpty(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                policies = policies.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var allAcks = (await _unitOfWork.PolicyAcknowledgements.GetAll())
                .Where(a => !a.IsDeleted)
                .ToList();

            var result = new List<PolicyDto>();
            foreach (var p in policies.OrderBy(p => p.Category).ThenBy(p => p.Title))
            {
                var pAcks = allAcks.Where(a => a.PolicyId == p.Id && a.PolicyVersion == p.Version).ToList();
                var userAck = currentEmployeeId.HasValue ? pAcks.FirstOrDefault(a => a.EmployeeId == currentEmployeeId.Value) : null;

                result.Add(new PolicyDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Category = p.Category,
                    ContentHtml = p.ContentHtml,
                    Version = p.Version,
                    IsMandatoryAcknowledgement = p.IsMandatoryAcknowledgement,
                    EffectiveDate = p.EffectiveDate,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate ?? DateTime.UtcNow,
                    IsAcknowledgedByCurrentUser = userAck != null,
                    UserAcknowledgedDate = userAck?.AcknowledgedDate,
                    TotalAcknowledgementsCount = pAcks.Count
                });
            }

            return result;
        }

        public async Task<PolicyDto?> GetPolicyByIdAsync(long id, long? currentEmployeeId = null)
        {
            var p = await _unitOfWork.CompanyPolicies.GetById(id);
            if (p == null || p.IsDeleted) return null;

            var acks = (await _unitOfWork.PolicyAcknowledgements.GetAll())
                .Where(a => a.PolicyId == p.Id && a.PolicyVersion == p.Version && !a.IsDeleted)
                .ToList();

            var userAck = currentEmployeeId.HasValue ? acks.FirstOrDefault(a => a.EmployeeId == currentEmployeeId.Value) : null;

            return new PolicyDto
            {
                Id = p.Id,
                Title = p.Title,
                Category = p.Category,
                ContentHtml = p.ContentHtml,
                Version = p.Version,
                IsMandatoryAcknowledgement = p.IsMandatoryAcknowledgement,
                EffectiveDate = p.EffectiveDate,
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate ?? DateTime.UtcNow,
                IsAcknowledgedByCurrentUser = userAck != null,
                UserAcknowledgedDate = userAck?.AcknowledgedDate,
                TotalAcknowledgementsCount = acks.Count
            };
        }

        public async Task<CompanyPolicy> CreatePolicyAsync(CreateCompanyPolicyRequest request, long createdBy)
        {
            var policy = new CompanyPolicy
            {
                Title = request.Title.Trim(),
                Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
                ContentHtml = request.ContentHtml,
                Version = 1,
                IsMandatoryAcknowledgement = request.IsMandatoryAcknowledgement,
                EffectiveDate = request.EffectiveDate > DateTime.MinValue ? request.EffectiveDate : DateTime.UtcNow.Date,
                IsActive = true,
                CreatedBy = (int)createdBy,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.CompanyPolicies.Add(policy);
            _unitOfWork.Save();
            return policy;
        }

        public async Task<CompanyPolicy> UpdatePolicyAsync(UpdateCompanyPolicyRequest request, long updatedBy)
        {
            var policy = await _unitOfWork.CompanyPolicies.GetById(request.Id);
            if (policy == null || policy.IsDeleted) throw new KeyNotFoundException($"Policy {request.Id} not found");

            policy.Title = request.Title.Trim();
            policy.Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
            policy.ContentHtml = request.ContentHtml;
            policy.IsMandatoryAcknowledgement = request.IsMandatoryAcknowledgement;
            policy.EffectiveDate = request.EffectiveDate > DateTime.MinValue ? request.EffectiveDate : policy.EffectiveDate;
            policy.IsActive = request.IsActive;

            if (request.IncrementVersion)
            {
                policy.Version += 1;
            }

            policy.UpdatedBy = (int)updatedBy;
            policy.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.CompanyPolicies.Update(policy);
            _unitOfWork.Save();
            return policy;
        }

        public async Task<bool> DeletePolicyAsync(long id)
        {
            var policy = await _unitOfWork.CompanyPolicies.GetById(id);
            if (policy == null || policy.IsDeleted) return false;

            policy.IsDeleted = true;
            policy.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.CompanyPolicies.Update(policy);
            _unitOfWork.Save();
            return true;
        }

        public async Task<PolicyAcknowledgement> AcknowledgePolicyAsync(long policyId, int version, long employeeId, string? ipAddress)
        {
            var policy = await _unitOfWork.CompanyPolicies.GetById(policyId);
            if (policy == null || policy.IsDeleted) throw new KeyNotFoundException($"Policy {policyId} not found");

            var existing = (await _unitOfWork.PolicyAcknowledgements.GetAll())
                .FirstOrDefault(a => a.PolicyId == policyId && a.PolicyVersion == version && a.EmployeeId == employeeId && !a.IsDeleted);

            if (existing != null)
            {
                return existing;
            }

            var ack = new PolicyAcknowledgement
            {
                PolicyId = policyId,
                PolicyVersion = version,
                EmployeeId = employeeId,
                AcknowledgedDate = DateTime.UtcNow,
                IpAddress = ipAddress,
                CreatedBy = (int)employeeId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.PolicyAcknowledgements.Add(ack);
            _unitOfWork.Save();
            return ack;
        }

        public async Task<List<PolicyAcknowledgement>> GetPolicyAcknowledgementsAsync(long policyId)
        {
            var acks = (await _unitOfWork.PolicyAcknowledgements.GetAll())
                .Where(a => a.PolicyId == policyId && !a.IsDeleted)
                .OrderByDescending(a => a.AcknowledgedDate)
                .ToList();

            var employees = (await _unitOfWork.Employees.GetAll()).ToList();
            foreach (var a in acks)
            {
                a.Employee = employees.FirstOrDefault(e => e.Id == a.EmployeeId);
            }

            return acks;
        }

        public async Task<PolicyComplianceSummaryDto> GetComplianceSummaryAsync()
        {
            var activePolicies = (await _unitOfWork.CompanyPolicies.GetAll())
                .Where(p => p.IsActive && !p.IsDeleted)
                .ToList();

            var mandatoryPolicies = activePolicies.Where(p => p.IsMandatoryAcknowledgement).ToList();

            var activeEmployees = (await _unitOfWork.Employees.GetAll())
                .Where(e => e.IsActive && !e.IsDeleted)
                .ToList();

            var allAcks = (await _unitOfWork.PolicyAcknowledgements.GetAll())
                .Where(a => !a.IsDeleted)
                .ToList();

            int totalEmployees = activeEmployees.Count;
            int fullyCompliantCount = 0;

            if (mandatoryPolicies.Any() && totalEmployees > 0)
            {
                foreach (var emp in activeEmployees)
                {
                    bool isFullyCompliant = true;
                    foreach (var mp in mandatoryPolicies)
                    {
                        bool hasAck = allAcks.Any(a => a.PolicyId == mp.Id && a.PolicyVersion == mp.Version && a.EmployeeId == emp.Id);
                        if (!hasAck)
                        {
                            isFullyCompliant = false;
                            break;
                        }
                    }
                    if (isFullyCompliant) fullyCompliantCount++;
                }
            }
            else if (!mandatoryPolicies.Any())
            {
                fullyCompliantCount = totalEmployees;
            }

            var perPolicy = new List<PolicyCompliancePerPolicyDto>();
            foreach (var p in activePolicies)
            {
                int ackCount = allAcks.Count(a => a.PolicyId == p.Id && a.PolicyVersion == p.Version);
                int pendingCount = Math.Max(0, totalEmployees - ackCount);
                decimal compliance = totalEmployees > 0 ? Math.Round((decimal)ackCount / totalEmployees * 100m, 1) : 100m;

                perPolicy.Add(new PolicyCompliancePerPolicyDto
                {
                    PolicyId = p.Id,
                    Title = p.Title,
                    Category = p.Category,
                    Version = p.Version,
                    AcknowledgedCount = ackCount,
                    PendingCount = pendingCount,
                    CompliancePercent = compliance
                });
            }

            decimal overallRate = totalEmployees > 0 ? Math.Round((decimal)fullyCompliantCount / totalEmployees * 100m, 1) : 100m;

            return new PolicyComplianceSummaryDto
            {
                TotalActivePolicies = activePolicies.Count,
                MandatoryPoliciesCount = mandatoryPolicies.Count,
                TotalEmployeesCount = totalEmployees,
                FullyCompliantEmployeesCount = fullyCompliantCount,
                OverallComplianceRatePercent = overallRate,
                PoliciesSummary = perPolicy
            };
        }
    }
}
