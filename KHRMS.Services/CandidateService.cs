using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;

namespace KHRMS.Services
{
    public class CandidateService(
        IUnitOfWork unitOfWork,
        ISendEmailService? sendEmailService = null,
        ITokenService? tokenService = null
    ) : ICandidateService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ISendEmailService? _sendEmailService = sendEmailService;
        private readonly ITokenService? _tokenService = tokenService;
        public async Task<bool> CreateCandidate(Candidate candidate)
        {
            if (candidate != null)
            {
                candidate.CreatedDate = DateTime.Now;
                await _unitOfWork.Candidates.Add(candidate);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public async Task<bool> DeleteCandidate(long candidateId)
        {
            if (candidateId > 0)
            {
                var candidateDetails = await _unitOfWork.Candidates.GetById(candidateId);
                if (candidateDetails != null)
                {
                    candidateDetails.IsDeleted = true;
                    candidateDetails.IsActive = false;
                    
                    _unitOfWork.Candidates.Update(candidateDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
        public async Task<IEnumerable<Candidate>> GetAllCandidates()
        {
            var candidates = await _unitOfWork.Candidates.GetAll();
            return candidates;
        }
        public async Task<Candidate> GetCandidateById(int candidateId)
        {
            if (candidateId > 0)
            {
                var candidateDetails = await _unitOfWork.Candidates.GetById(candidateId);
                if (candidateDetails != null)
                {
                    return candidateDetails;
                }
            }
            return null;
        }
        public async Task<bool> UpdateCandidate(Candidate candidate)
        {
            if (candidate != null)
            {
                var candidateDetails = await _unitOfWork.Candidates.GetById(candidate.Id);
                if (candidateDetails != null)
                {
                    candidateDetails.FirstName = candidate.FirstName;
                    candidateDetails.LastName = candidate.LastName;
                    candidateDetails.EmailAddress = candidate.EmailAddress;
                    candidateDetails.MobileNumber = candidate.MobileNumber;
                    candidateDetails.CurrentSalary = candidate.CurrentSalary;
                    candidateDetails.ExpectedSalary = candidate.ExpectedSalary;
                    candidateDetails.TotalExperience = candidate.TotalExperience;
                    candidateDetails.RelevantExperience = candidate.RelevantExperience;
                    candidateDetails.NoticePeriod = candidate.NoticePeriod;
                    candidateDetails.UpdatedDate = DateTime.Now;
                    candidateDetails.AppliedRole = candidate.AppliedRole;
                    candidateDetails.Stage = candidate.Stage;
                    candidateDetails.MatchScore = candidate.MatchScore;
                    // ✅ Ensure IsActive status is updated
                    candidateDetails.IsActive = candidate.IsActive;
                    _unitOfWork.Candidates.Update(candidateDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public async Task<CandidateOnboardResponse> OnboardCandidate(CandidateOnboardRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var now = DateTime.Now;

            // 1. Update Candidate stage to "Onboarded"
            Candidate? candidate = null;
            if (request.CandidateId > 0)
            {
                candidate = await _unitOfWork.Candidates.GetById(request.CandidateId);
            }
            if (candidate == null && !string.IsNullOrWhiteSpace(request.EmailAddress))
            {
                var allCands = await _unitOfWork.Candidates.GetAll();
                candidate = allCands.FirstOrDefault(c => c.EmailAddress != null && c.EmailAddress.Equals(request.EmailAddress.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            if (candidate != null)
            {
                candidate.Stage = "Onboarded";
                candidate.UpdatedDate = now;
                if (!string.IsNullOrWhiteSpace(candidate.RelevantExperience) && candidate.RelevantExperience.Trim().StartsWith("{"))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(candidate.RelevantExperience);
                        var dict = new Dictionary<string, object?>();
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            dict[prop.Name] = prop.Value.ValueKind switch
                            {
                                JsonValueKind.String => prop.Value.GetString(),
                                JsonValueKind.Number => prop.Value.GetDouble(),
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                JsonValueKind.Null => null,
                                _ => JsonSerializer.Deserialize<object>(prop.Value.GetRawText())
                            };
                        }
                        dict["status"] = "Onboarded";
                        dict["interviewerId"] = null;
                        dict["interviewerName"] = null;
                        candidate.RelevantExperience = JsonSerializer.Serialize(dict);
                    }
                    catch { }
                }
                _unitOfWork.Candidates.Update(candidate);
                _unitOfWork.Save();
            }

            // 2. Structured CTC Breakdown JSON stored in Responsibilities
            var ctcObj = new
            {
                annualCtc = request.AnnualCtc,
                basicSalary = request.BasicSalary,
                hra = request.Hra,
                specialAllowance = request.SpecialAllowance,
                pfDeduction = request.PfDeduction,
                netMonthlySalary = request.NetMonthlySalary,
                currency = "INR",
                updatedAt = DateTime.UtcNow
            };
            string ctcJson = JsonSerializer.Serialize(ctcObj);

            // 3. Create or Update Employee
            var existingEmp = (await _unitOfWork.Employees.GetAll())
                .FirstOrDefault(e => e.EmailAddress != null &&
                    e.EmailAddress.Equals(request.EmailAddress, StringComparison.OrdinalIgnoreCase) &&
                    e.IsDeleted != true);

            Employee employee;
            if (existingEmp == null)
            {
                employee = new Employee
                {
                    FirstName = request.FirstName?.Trim(),
                    LastName = request.LastName?.Trim(),
                    EmailAddress = request.EmailAddress?.Trim(),
                    PrimaryEmailAddress = request.EmailAddress?.Trim(),
                    MobileNumber = request.MobileNumber?.Trim(),
                    DesignationId = request.DesignationId > 0 ? request.DesignationId : null,
                    Designation = request.Designation,
                    Branch = request.Department ?? "Engineering",
                    DateOfJoining = request.DateOfJoining ?? now,
                    Gender = !string.IsNullOrWhiteSpace(request.Gender) ? request.Gender : "Male",
                    ManagerId = request.ManagerId > 0 ? request.ManagerId : null,
                    ShiftIds = !string.IsNullOrWhiteSpace(request.ShiftId) ? request.ShiftId : "1",
                    Responsibilities = ctcJson,
                    ProfileCompleted = false,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = now,
                    EmployeeCode = 0
                };
                await _unitOfWork.Employees.Add(employee);
                _unitOfWork.Save();

                employee.EmployeeCode = employee.Id;
                _unitOfWork.Employees.Update(employee);
                _unitOfWork.Save();
            }
            else
            {
                employee = existingEmp;
                if (request.DesignationId > 0) employee.DesignationId = request.DesignationId;
                if (!string.IsNullOrWhiteSpace(request.Designation)) employee.Designation = request.Designation;
                if (!string.IsNullOrWhiteSpace(request.Department)) employee.Branch = request.Department;
                if (request.DateOfJoining.HasValue) employee.DateOfJoining = request.DateOfJoining.Value;
                if (request.ManagerId > 0) employee.ManagerId = request.ManagerId;
                if (!string.IsNullOrWhiteSpace(request.ShiftId)) employee.ShiftIds = request.ShiftId;
                if (!string.IsNullOrWhiteSpace(request.MobileNumber)) employee.MobileNumber = request.MobileNumber;
                employee.Responsibilities = ctcJson;
                employee.IsActive = true;
                employee.IsDeleted = false;
                employee.UpdatedDate = now;
                _unitOfWork.Employees.Update(employee);
                _unitOfWork.Save();
            }

            // 4. Assign Employee Role
            var allRoles = (await _unitOfWork.RoleMaster.GetAll())
                .Where(r => r.IsActive != false && r.IsDeleted != true)
                .ToList();

            var targetRole = allRoles.FirstOrDefault(r =>
                (request.RoleId.HasValue && r.Id == request.RoleId.Value) ||
                (!string.IsNullOrEmpty(request.RoleName) && string.Equals(r.RoleName, request.RoleName, StringComparison.OrdinalIgnoreCase)) ||
                string.Equals(r.RoleName, "Employee", StringComparison.OrdinalIgnoreCase)
            );

            if (targetRole != null)
            {
                var existingRoleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                    .Where(m => m.EmployeeId == employee.Id && m.IsActive != false)
                    .ToList();

                if (!existingRoleMappings.Any(m => m.RoleId == targetRole.Id))
                {
                    await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                    {
                        EmployeeId = employee.Id,
                        RoleId = targetRole.Id,
                        IsActive = true,
                        CreatedDate = now
                    });
                    _unitOfWork.Save();
                }
            }

            // 5. User Registration & User Login
            string tempPassword = !string.IsNullOrWhiteSpace(request.TemporaryPassword)
                ? request.TemporaryPassword
                : $"Hrms@{DateTime.Now.Year}!";

            var passwordHasher = new PasswordHasher<UserRegistration>();
            string hashedPassword = passwordHasher.HashPassword(null, tempPassword);

            var existingLogin = (await _unitOfWork.UserLogins.GetAll())
                .FirstOrDefault(u => u.Email != null && u.Email.Equals(employee.EmailAddress, StringComparison.OrdinalIgnoreCase));

            if (existingLogin == null)
            {
                var userLogin = new UserLogin
                {
                    UserId = employee.Id,
                    UserName = employee.EmailAddress,
                    Email = employee.EmailAddress,
                    Password = hashedPassword,
                    CreatedDate = now,
                    IsActive = true,
                    IsDeleted = false,
                    IsResetPasswordRequired = true
                };
                await _unitOfWork.UserLogins.Add(userLogin);
                _unitOfWork.Save();
            }

            // 6. Send Onboarding Credentials Email
            bool emailSent = false;
            if (request.SendCredentialsEmail && _sendEmailService != null)
            {
                try
                {
                    var companyName = "Krishiv Innovations";
                    string token = _tokenService != null ? _tokenService.GeneratePasswordResetToken(employee.EmailAddress) : string.Empty;
                    var clientUrl = !string.IsNullOrEmpty(request.ClientUrl) ? request.ClientUrl : "http://localhost:4200/login";
                    var queryParams = new Dictionary<string, string>
                    {
                        { "Token", token },
                        { "Email", employee.EmailAddress }
                    };
                    var fullLoginUrl = QueryHelpers.AddQueryString(clientUrl, queryParams);
                    var placeholders = new Dictionary<string, string>
                    {
                        { "COMPANY_NAME", companyName },
                        { "USERNAME", employee.EmailAddress },
                        { "TEMP_PASSWORD", tempPassword },
                        { "URL", fullLoginUrl },
                        { "SUPPORT_EMAIL", "hr@krishivinnovations.com" },
                        { "HR_OR_IT_TEAM_NAME", "HR Operations" }
                    };
                    var subject = $"Welcome to {companyName} – Your Account Access Details";
                    emailSent = await _sendEmailService.SendTemplateEmailAsync(employee.EmailAddress, subject, placeholders, "employee_onboarding_credentials");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to send onboarding credentials email to {Email}", employee.EmailAddress);
                    emailSent = false;
                }
            }

            return new CandidateOnboardResponse
            {
                EmployeeId = employee.Id,
                Email = employee.EmailAddress,
                TemporaryPassword = tempPassword,
                FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
                EmailSent = emailSent
            };
        }
    }
}
