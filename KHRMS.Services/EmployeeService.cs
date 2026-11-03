using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using KHRMS.Core;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata; // Add this for PasswordHasher

namespace KHRMS.Services
{
    public class EmployeeService(
        IUnitOfWork unitOfWork,
        ISendEmailService emailRepository,
        ITokenService tokenService,
        IAuditService auditService,
        IUserContextService? userContextService = null) : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ISendEmailService _sendEmailService = emailRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IAuditService _auditService = auditService;
        private readonly IUserContextService? _userContextService = userContextService;
        private const string AllowdChar = "abcdefghijklmnopqrstuvwxyz" +
                                          "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                          "0123456789" +
                                          "@#";
        private string GenerateRandomPassword(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            string password = RandomNumberGenerator.GetString(AllowdChar, length);
            return password;
        }
        //public async Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel)
        //{
        //    if (employeeRequestModel == null)
        //        return false;
        //    // Find the most senior employee to assign as a manager
        //    var seniorEmployee = (await _unitOfWork.Employees.GetAll())
        //        .OrderBy(emp => emp.DateOfJoining)  // Oldest employee (seniority)
        //        .FirstOrDefault();
        //    var newEmployee = new Employee
        //    {
        //        EmployeeCode = employeeRequestModel.EmployeeCode,
        //        FirstName = employeeRequestModel.FirstName,
        //        LastName = employeeRequestModel.LastName,
        //        EmailAddress = employeeRequestModel.EmailAddress,
        //        MobileNumber = employeeRequestModel.MobileNumber,
        //        DesignationId = employeeRequestModel.DesignationId,
        //        DateOfJoining = employeeRequestModel.DateOfJoining,
        //        Gender = employeeRequestModel.Gender,
        //        CurrentAddress = employeeRequestModel.CurrentAddress,
        //        PermanentAddress = employeeRequestModel.PermanentAddress,
        //        IsActive = employeeRequestModel.IsActive,
        //        CreatedDate = DateTime.Now,
        //        ShiftIds = employeeRequestModel.ShiftId,
        //    };
        //    await _unitOfWork.Employees.Add(newEmployee);
        //    var result = _unitOfWork.Save();
        //    if (result > 0 && employeeRequestModel.RoleIds?.Any() == true)
        //    {
        //        var roleMappings = employeeRequestModel.RoleIds.Select(roleId => new EmployeeRoleMapping
        //        {
        //            EmployeeId = newEmployee.Id,
        //            RoleId = roleId,
        //            IsActive = true,
        //            CreatedDate = DateTime.Now
        //        }).ToList();

        //        foreach (var roleMapping in roleMappings)
        //        {
        //            await _unitOfWork.EmployeeRoleMappings.Add(roleMapping);
        //        }
        //        _unitOfWork.Save();
        //    }
        //    return result > 0;
        //}

        //public async Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel)
        //{
        //    if (employeeRequestModel == null)
        //        return false;

        //    var seniorEmployee = (await _unitOfWork.Employees.GetAll())
        //        .OrderBy(emp => emp.DateOfJoining)
        //        .FirstOrDefault();

        //    var newEmployee = new Employee
        //    {
        //        EmployeeCode = employeeRequestModel.EmployeeCode,
        //        FirstName = employeeRequestModel.FirstName,
        //        LastName = employeeRequestModel.LastName,
        //        EmailAddress = employeeRequestModel.EmailAddress,
        //        MobileNumber = employeeRequestModel.MobileNumber,
        //        PrimaryEmailAddress = employeeRequestModel.PrimaryEmailAddress, 
        //        DesignationId = employeeRequestModel.DesignationId,
        //        DateOfJoining = employeeRequestModel.DateOfJoining,
        //        Gender = employeeRequestModel.Gender,
        //        CurrentAddress = employeeRequestModel.CurrentAddress,
        //        PermanentAddress = employeeRequestModel.PermanentAddress,
        //        IsActive = employeeRequestModel.IsActive,
        //        CreatedDate = DateTime.Now,
        //        ShiftIds = employeeRequestModel.ShiftId,
        //    };

        //    await _unitOfWork.Employees.Add(newEmployee);
        //    var result = _unitOfWork.Save();


        //    // Add UserRegistration
        //    var defaultPassword = "System@123";
        //    var passwordHasher = new PasswordHasher<UserRegistration>();

        //    var userRegistration = new UserRegistration
        //    {
        //        FirstName = newEmployee.FirstName,
        //        LastName = newEmployee.LastName,
        //        Email = newEmployee.EmailAddress,
        //        MobileNumber = newEmployee.MobileNumber,
        //        Address = newEmployee.CurrentAddress,
        //        Password = passwordHasher.HashPassword(null, defaultPassword),
        //        CreatedDate = DateTime.Now
        //    };

        //    await _unitOfWork.UserRegistrations.Add(userRegistration);
        //    var regResult = _unitOfWork.Save();

        //    // Add UserLogin
        //    if (regResult > 0)
        //    {
        //        var userLogin = new KHRMS.Core.UserLogin
        //        {
        //            UserId = userRegistration.Id,
        //            UserName = userRegistration.Email,
        //            Email = userRegistration.Email,
        //            Password = userRegistration.Password,
        //            CreatedDate = DateTime.Now,
        //            IsActive = true,
        //            IsDeleted = false
        //        };

        //        await _unitOfWork.UserLogins.Add(userLogin);
        //        _unitOfWork.Save();
        //    }

        //    return result > 0;
        //}
        public async Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel)
        {
            if (employeeRequestModel == null)
                return false;

            // Get senior employee (best possible with current repo)
            var seniorEmployee = (await _unitOfWork.Employees.GetAll())
                .MinBy(e => e.DateOfJoining);

            var now = DateTime.Now;

            long assignedEmpCode = employeeRequestModel.EmployeeCode ?? 0;
            if (assignedEmpCode <= 0)
            {
                var existingCodes = (await _unitOfWork.Employees.GetAll())
                    .Select(e => e.EmployeeCode)
                    .Where(c => c > 0)
                    .ToList();
                assignedEmpCode = existingCodes.Any() ? (existingCodes.Max() ?? 100) + 1 : 101;
            }

            var newEmployee = new Employee
            {
                FirstName = employeeRequestModel.FirstName,
                LastName = employeeRequestModel.LastName,
                EmailAddress = employeeRequestModel.EmailAddress,
                MobileNumber = !string.IsNullOrWhiteSpace(employeeRequestModel.MobileNumber)
                    ? System.Text.RegularExpressions.Regex.Replace(employeeRequestModel.MobileNumber, @"[^\d]", "").Substring(0, Math.Min(10, System.Text.RegularExpressions.Regex.Replace(employeeRequestModel.MobileNumber, @"[^\d]", "").Length))
                    : null,
                DateOfJoining = employeeRequestModel.DateOfJoining,
                SkillIds = employeeRequestModel.SkillIds,
                ProjectIds = employeeRequestModel.ProjectIds,
                DesignationId = employeeRequestModel.DesignationId ?? 0,
                ManagerId = employeeRequestModel.ManagerId,
                CreatedDate = now,
                CreatedBy = employeeRequestModel.CreatedBy,
                IsActive = employeeRequestModel.IsActive,
                IsDeleted = false,
                EmployeeCode = assignedEmpCode,
                ProfileCompleted = false,
                ProbationEndDate = employeeRequestModel.ProbationEndDate ?? employeeRequestModel.DateOfJoining?.AddMonths(3) ?? now.AddMonths(3),
                ProbationStatus = !string.IsNullOrWhiteSpace(employeeRequestModel.ProbationStatus) ? employeeRequestModel.ProbationStatus : "UnderProbation",
                EmploymentStatus = !string.IsNullOrWhiteSpace(employeeRequestModel.EmploymentStatus) ? employeeRequestModel.EmploymentStatus : "Probation",
                EmploymentType = !string.IsNullOrWhiteSpace(employeeRequestModel.EmploymentType) ? employeeRequestModel.EmploymentType : "FullTime"
            };

            await _unitOfWork.Employees.Add(newEmployee);
            var res = _unitOfWork.Save();

            // Role mappings
            if (res > 0 && employeeRequestModel.RoleIds?.Any() == true)
            {
                foreach (var roleId in employeeRequestModel.RoleIds)
                {
                    await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                    {
                        EmployeeId = newEmployee.Id,
                        RoleId = roleId,
                        IsActive = true,
                        CreatedDate = now
                    });
                }
            }

            // Save employee + roles together
            var result = _unitOfWork.Save();
            if (result <= 0)
                return false;

            // User registration
            var defaultPassword = GenerateRandomPassword(12);
            var passwordHasher = new PasswordHasher<UserRegistration>();

            // User login
            var userLogin = new UserLogin
            {
                UserId = newEmployee.Id,
                UserName = newEmployee.EmailAddress,
                Email = newEmployee.EmailAddress,
                Password = passwordHasher.HashPassword(null, defaultPassword),
                CreatedDate = now,
                IsActive = true,
                IsDeleted = false,
                IsResetPasswordRequired = true
            };

            await _unitOfWork.UserLogins.Add(userLogin);
            if(_unitOfWork.Save() > 0)
            {
                var CompanyName = "Krishiv Innovations";
                var TeamName = "HR";
                var token = _tokenService.GeneratePasswordResetToken(userLogin.Email);
                var SupportEmail = "hr@krishivinnovations.com";
                    var url = new Dictionary<string, string>
                    {
                        {"Token",token },
                        {"Email" ,userLogin.Email}
                    };
                    var req = QueryHelpers.AddQueryString(employeeRequestModel.ClientUrl!, url);
                    var dict = new Dictionary<string, string>
                    {
                        {"COMPANY_NAME",CompanyName},
                        {"USERNAME",userLogin.Email },
                        {"TEMP_PASSWORD" ,defaultPassword},
                        {"URL" , req},
                        {"SUPPORT_EMAIL" ,SupportEmail},
                        {"HR_OR_IT_TEAM_NAME" ,TeamName},
                    };
                    var subject = $"Welcome to {CompanyName} – Your Account Access Details";

                    await _sendEmailService.SendTemplateEmailAsync(userLogin.Email, subject, dict, "employee_onboarding_credentials");
                    return true;
            }

            // Save user-related entities together
            return _unitOfWork.Save() > 0;

        }

        public async Task<bool> DeleteEmployee(long employeeId)
        {
            if (employeeId <= 0) return false;

            var employeeDetails = await _unitOfWork.Employees.GetById(employeeId);
            if (employeeDetails == null || employeeDetails.IsDeleted) return false;

            // Safeguard 1: Block deleting the last active Administrator
            var allRoles = (await _unitOfWork.RoleMaster.GetAll()).ToList();
            var adminRoleIds = allRoles
                .Where(r => string.Equals(r.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(r.RoleName, "System Admin", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Id)
                .ToHashSet();

            var activeAdminMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => adminRoleIds.Contains(m.RoleId) && m.IsActive && !m.IsDeleted)
                .ToList();

            bool isTargetAdmin = activeAdminMappings.Any(m => m.EmployeeId == employeeId);
            if (isTargetAdmin)
            {
                var otherActiveAdmins = activeAdminMappings
                    .Where(m => m.EmployeeId != employeeId)
                    .Select(m => m.EmployeeId)
                    .Distinct()
                    .ToList();

                if (!otherActiveAdmins.Any())
                {
                    throw new InvalidOperationException("Cannot delete or deactivate the last active System Administrator.");
                }
            }

            var beforeSnapshot = new
            {
                employeeDetails.Id,
                employeeDetails.EmployeeCode,
                employeeDetails.FirstName,
                employeeDetails.LastName,
                employeeDetails.EmailAddress,
                employeeDetails.IsActive
            };

            employeeDetails.IsDeleted = true;
            employeeDetails.IsActive = false;
            employeeDetails.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Employees.Update(employeeDetails);

            // Deactivate linked UserLogins so deleted employee cannot log in
            var userLogins = (await _unitOfWork.UserLogins.GetAll())
                .Where(u => u.UserId == employeeId || string.Equals(u.Email, employeeDetails.EmailAddress, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var ul in userLogins)
            {
                ul.IsDeleted = true;
                ul.IsActive = false;
                ul.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.UserLogins.Update(ul);
            }

            // Deactivate assigned roles
            var assignedRoles = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.EmployeeId == employeeId && m.IsActive && !m.IsDeleted)
                .ToList();

            foreach (var ar in assignedRoles)
            {
                ar.IsActive = false;
                ar.IsDeleted = true;
                ar.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.EmployeeRoleMappings.Update(ar);
            }

            var result = _unitOfWork.Save();

            if (result > 0)
            {
                await _auditService.LogAsync(
                    moduleName: "Workforce",
                    entityName: "Employee",
                    entityId: employeeId.ToString(),
                    action: "DELETE",
                    beforeValues: beforeSnapshot,
                    afterValues: null,
                    reason: "Employee offboarded and deleted"
                );
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<EmployeeRequestModel>> GetAllEmployees()
        {
            var employees = await _unitOfWork.Employees.GetAll();
            var employeeroleMapping = await _unitOfWork.EmployeeRoleMappings.GetAll();
            var allSkillIds = employees
                .Where(e => e.SkillIds != null)
                .SelectMany(e => e.SkillIds)
                .Distinct()
                .ToList();
            var skills = (await _unitOfWork.Skills.GetAll()).Where(s=>allSkillIds.Contains(s.Id)).ToDictionary(s=>s.Id, s=>s.SkillName);
            var rolemaster = (await _unitOfWork.RoleMaster.GetAll()).ToDictionary(role => role.Id);
            var projects = (await _unitOfWork.ProjectMasters.GetAll()).ToDictionary(p => p.Id, p => p.ProjectName);
            var employeeDictionary = employees.ToDictionary(emp => emp.Id);

            var employeesWithRoles = employees.Select(emp => new EmployeeRequestModel
            {
                Id = emp.Id,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                ProfileImage = emp.ProfileImage,
                EmailAddress = emp.EmailAddress,
                EmployeeCode = emp.EmployeeCode,
                MobileNumber = emp.MobileNumber,
                DesignationId =emp.DesignationId,
                DateOfJoining = emp.DateOfJoining,
                Gender = emp.Gender,
                CurrentAddress = emp.CurrentAddress,
                PermanentAddress = emp.PermanentAddress,
                IsActive = emp.IsActive,
                ManagerId = emp?.ManagerId,
                CreatedDate = emp.CreatedDate,
                ShiftId = emp.ShiftIds,
                PrimaryEmailAddress = emp.PrimaryEmailAddress,
                PrimaryContactName = emp.PrimaryContactName,
                PrimaryContactRelationship = emp.PrimaryContactRelationship,
                PrimaryContactPhone = emp.PrimaryContactPhone,
                PrimaryContactEmail = emp.PrimaryContactEmail,
                PrimaryContactAddress = emp.PrimaryContactAddress,
                SecondaryContactName = emp.SecondaryContactName,
                SecondaryContactRelationship = emp.SecondaryContactRelationship,
                SecondaryContactPhone = emp.SecondaryContactPhone,
                SecondaryContactEmail = emp.SecondaryContactEmail,
                SecondaryContactAddress = emp.SecondaryContactAddress,
                Degree = emp.Degree,
                University = emp.University,
                YearOfPassing = emp.YearOfPassing,
                Percentage = emp.Percentage,
                CompanyName = emp.CompanyName,
                Designation = emp.Designation,
                ExperienceDuration = emp.ExperienceDuration,
                ExperienceLocation = emp.ExperienceLocation,
                Responsibilities = emp.Responsibilities,
                PassportNumber = emp.PassportNumber,
                Nationality = emp.Nationality,
                PassportIssueDate = emp.PassportIssueDate,
                PassportExpiryDate = emp.PassportExpiryDate,
                PassportScanCopy = emp.PassportScanCopy,
                Branch = emp.Branch,
                DateOfBirth = emp.DateOfBirth,
                SkillIds = emp.SkillIds ?? new List<long>(),
                Skills = (emp.SkillIds ?? Enumerable.Empty<long>())
                    .Where(id=>skills.ContainsKey(id))
                    .Select(id => skills[id]).ToList(),
                ProjectIds = emp.ProjectIds ?? new List<long>(),
                Projects = (emp.ProjectIds ?? Enumerable.Empty<long>())
                    .Where(id => projects.ContainsKey(id))
                    .Select(id => projects[id]).ToList(),
                RoleIds = employeeroleMapping
                    .Where(mapping => mapping.EmployeeId == emp.Id && mapping.IsActive)
                    .Select(mapping => mapping.RoleId)
                    .Where(roleId => rolemaster.ContainsKey(roleId))
                    .ToList(),
                rolenames = employeeroleMapping
                    .Where(mapping => mapping.EmployeeId == emp.Id && mapping.IsActive)
                    .Select(mapping => mapping.RoleId)
                    .Where(roleId => rolemaster.ContainsKey(roleId))
                    .Select(roleId => rolemaster[roleId].RoleName)
                    .ToList(),
                ManagerName = (emp.ManagerId.HasValue && employeeDictionary.TryGetValue(emp.ManagerId.Value, out var mgr))
                    ? $"{mgr.FirstName} {mgr.LastName}"
                    : "Manager Not Assigned",
                ProbationEndDate = emp.ProbationEndDate,
                ProbationStatus = emp.ProbationStatus ?? "Confirmed",
                EmploymentStatus = emp.EmploymentStatus ?? (emp.IsActive ? "Active" : "Exited"),
                EmploymentType = emp.EmploymentType ?? "FullTime",
                RehireDate = emp.RehireDate,
                ExitDate = emp.ExitDate

            }).OrderByDescending(emp => emp.CreatedDate) // Sort new entries at the top
    .ToList();



            return employeesWithRoles;
        }

        public async Task<IEnumerable<EmployeeLookupDto>> GetEmployeesLookupAsync()
        {
            var employees = await _unitOfWork.Employees.GetAll();
            var designations = (await _unitOfWork.Designations.GetAll())
                .ToDictionary(d => d.Id, d => d.DesignationName);

            var lookupList = employees
                .Where(e => !e.IsDeleted && e.IsActive)
                .Select(e => new EmployeeLookupDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName ?? "",
                    LastName = e.LastName ?? "",
                    Designation = (e.DesignationId.HasValue && designations.TryGetValue(e.DesignationId.Value, out var desName))
                        ? desName
                        : e.Designation,
                    DesignationId = e.DesignationId,
                    ProfileImage = e.ProfileImage,
                    EmailAddress = e.EmailAddress,
                    Department = e.Branch,
                    ManagerId = e.ManagerId,
                    IsActive = e.IsActive
                })
                .OrderBy(e => e.FirstName)
                .ToList();

            return lookupList;
        }

        public async Task<Employee> GetEmployeeById(int employeeId)
        {
            if (employeeId > 0)
            {
                var employeeDetails = await _unitOfWork.Employees.GetById(employeeId);
                if (employeeDetails != null)
                {
                    return employeeDetails;
                }
            }
            return null;
        }

        public async Task<bool> UpdateEmployee(EmployeeRequestModel employeeRequestModel)
        {
            if (employeeRequestModel == null)
                return false;

            var employeeDetails = await _unitOfWork.Employees.GetById(employeeRequestModel.Id);
            if (employeeDetails == null)
                return false;

            // Update Employee details
            // Update Employee details
            if (employeeRequestModel.ProfileCompleted.HasValue)
            {
                employeeDetails.ProfileCompleted = employeeRequestModel.ProfileCompleted.Value;
            }

            employeeDetails.EmployeeCode = employeeRequestModel.EmployeeCode ?? employeeDetails.EmployeeCode;
            employeeDetails.FirstName = employeeRequestModel.FirstName ?? employeeDetails.FirstName;
            employeeDetails.LastName = employeeRequestModel.LastName ?? employeeDetails.LastName;
            employeeDetails.ProfileImage = employeeRequestModel.ProfileImage ?? employeeDetails.ProfileImage;
            employeeDetails.EmailAddress = employeeRequestModel.EmailAddress ?? employeeDetails.EmailAddress;
            if (!string.IsNullOrWhiteSpace(employeeRequestModel.MobileNumber))
            {
                var digits = System.Text.RegularExpressions.Regex.Replace(employeeRequestModel.MobileNumber, @"[^\d]", "");
                employeeDetails.MobileNumber = digits.Length > 10 ? digits.Substring(0, 10) : digits;
            }
            employeeDetails.DesignationId = employeeRequestModel.DesignationId ?? employeeDetails.DesignationId;
            employeeDetails.DateOfJoining = employeeRequestModel.DateOfJoining ?? employeeDetails.DateOfJoining;
            employeeDetails.Gender = employeeRequestModel.Gender ?? employeeDetails.Gender;
            employeeDetails.CurrentAddress = employeeRequestModel.CurrentAddress ?? employeeDetails.CurrentAddress;
            employeeDetails.PermanentAddress = employeeRequestModel.PermanentAddress ?? employeeDetails.PermanentAddress;
            employeeDetails.IsActive = employeeRequestModel.IsActive;
            employeeDetails.UpdatedDate = DateTime.Now;
            employeeDetails.ShiftIds = employeeRequestModel.ShiftId ?? employeeDetails.ShiftIds;
            employeeDetails.ManagerId = employeeRequestModel.ManagerId ?? employeeDetails.ManagerId;
            employeeDetails.PrimaryEmailAddress = employeeRequestModel.PrimaryEmailAddress ?? employeeDetails.PrimaryEmailAddress;
            employeeDetails.PrimaryContactName = employeeRequestModel.PrimaryContactName ?? employeeDetails.PrimaryContactName;
            employeeDetails.PrimaryContactRelationship = employeeRequestModel.PrimaryContactRelationship ?? employeeDetails.PrimaryContactRelationship;
            if (!string.IsNullOrWhiteSpace(employeeRequestModel.PrimaryContactPhone))
            {
                var digits = System.Text.RegularExpressions.Regex.Replace(employeeRequestModel.PrimaryContactPhone, @"[^\d]", "");
                employeeDetails.PrimaryContactPhone = digits.Length > 15 ? digits.Substring(0, 15) : digits;
            }
            employeeDetails.PrimaryContactEmail = employeeRequestModel.PrimaryContactEmail ?? employeeDetails.PrimaryContactEmail;
            employeeDetails.PrimaryContactAddress = employeeRequestModel.PrimaryContactAddress ?? employeeDetails.PrimaryContactAddress;
            employeeDetails.SecondaryContactName = employeeRequestModel.SecondaryContactName ?? employeeDetails.SecondaryContactName;
            employeeDetails.SecondaryContactRelationship = employeeRequestModel.SecondaryContactRelationship ?? employeeDetails.SecondaryContactRelationship;
            if (!string.IsNullOrWhiteSpace(employeeRequestModel.SecondaryContactPhone))
            {
                var digits = System.Text.RegularExpressions.Regex.Replace(employeeRequestModel.SecondaryContactPhone, @"[^\d]", "");
                employeeDetails.SecondaryContactPhone = digits.Length > 15 ? digits.Substring(0, 15) : digits;
            }
            employeeDetails.SecondaryContactEmail = employeeRequestModel.SecondaryContactEmail ?? employeeDetails.SecondaryContactEmail;
            employeeDetails.SecondaryContactAddress = employeeRequestModel.SecondaryContactAddress ?? employeeDetails.SecondaryContactAddress;
            employeeDetails.Degree = employeeRequestModel.Degree ?? employeeDetails.Degree;
            employeeDetails.University = employeeRequestModel.University ?? employeeDetails.University;
            employeeDetails.YearOfPassing = employeeRequestModel.YearOfPassing ?? employeeDetails.YearOfPassing;
            employeeDetails.Percentage = employeeRequestModel.Percentage ?? employeeDetails.Percentage;
            employeeDetails.CompanyName = employeeRequestModel.CompanyName ?? employeeDetails.CompanyName;
            employeeDetails.Designation = employeeRequestModel.Designation ?? employeeDetails.Designation;
            employeeDetails.ExperienceDuration = employeeRequestModel.ExperienceDuration ?? employeeDetails.ExperienceDuration;
            employeeDetails.ExperienceLocation = employeeRequestModel.ExperienceLocation ?? employeeDetails.ExperienceLocation;
            employeeDetails.Responsibilities = employeeRequestModel.Responsibilities ?? employeeDetails.Responsibilities;
            employeeDetails.PassportNumber = employeeRequestModel.PassportNumber ?? employeeDetails.PassportNumber;
            employeeDetails.Nationality = employeeRequestModel.Nationality ?? employeeDetails.Nationality;
            employeeDetails.PassportIssueDate = employeeRequestModel.PassportIssueDate ?? employeeDetails.PassportIssueDate;
            employeeDetails.PassportExpiryDate = employeeRequestModel.PassportExpiryDate ?? employeeDetails.PassportExpiryDate;
            employeeDetails.PassportScanCopy = employeeRequestModel.PassportScanCopy ?? employeeDetails.PassportScanCopy;
            employeeDetails.Branch = employeeRequestModel.Branch ?? employeeDetails.Branch;
            employeeDetails.DateOfBirth = employeeRequestModel.DateOfBirth ?? employeeDetails.DateOfBirth;
            if (employeeRequestModel.SkillIds != null)
                employeeDetails.SkillIds = employeeRequestModel.SkillIds;
            if (employeeRequestModel.ProjectIds != null)
                employeeDetails.ProjectIds = employeeRequestModel.ProjectIds;
            if (employeeRequestModel.ProbationEndDate.HasValue) employeeDetails.ProbationEndDate = employeeRequestModel.ProbationEndDate.Value;
            if (!string.IsNullOrWhiteSpace(employeeRequestModel.ProbationStatus)) employeeDetails.ProbationStatus = employeeRequestModel.ProbationStatus;
            if (!string.IsNullOrWhiteSpace(employeeRequestModel.EmploymentStatus)) employeeDetails.EmploymentStatus = employeeRequestModel.EmploymentStatus;
            if (!string.IsNullOrWhiteSpace(employeeRequestModel.EmploymentType)) employeeDetails.EmploymentType = employeeRequestModel.EmploymentType;

            _unitOfWork.Employees.Update(employeeDetails);
            var saveEmployeeResult = _unitOfWork.Save();

                if (employeeRequestModel.RoleIds != null && employeeRequestModel.RoleIds.Any())
                {
                    var existingRoleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                        .Where(r => r.EmployeeId == employeeDetails.Id)
                        .ToList();
                    foreach (var roleMapping in existingRoleMappings.Where(r => r.IsActive))
                    {
                        roleMapping.IsActive = false;
                        roleMapping.UpdatedDate = DateTime.Now;
                        _unitOfWork.EmployeeRoleMappings.Update(roleMapping);
                    }

                    foreach (var roleId in employeeRequestModel.RoleIds)
                    {
                        var existingMapping = existingRoleMappings.FirstOrDefault(r => r.RoleId == roleId);

                        if (existingMapping != null)
                        {
                            existingMapping.IsActive = true;
                            existingMapping.UpdatedDate = DateTime.Now;
                            _unitOfWork.EmployeeRoleMappings.Update(existingMapping);
                        }
                        else
                        {
                            await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                            {
                                RoleId = roleId,
                                EmployeeId = employeeDetails.Id,
                                IsActive = true,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }
                    _unitOfWork.Save();
                }

                return true;
            }


        public async Task<IEnumerable<EmployeeRequestModel>> GetAllManagers()
        {
            var managerRole = (await _unitOfWork.RoleMaster.GetAll()).FirstOrDefault(r => r.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase));
            if (managerRole == null)
                return Enumerable.Empty<EmployeeRequestModel>();
            var employeeRoleMapping = (await _unitOfWork.EmployeeRoleMappings.GetAll()).Where(mapping => mapping.RoleId == managerRole.Id).ToList();
            var manageids = employeeRoleMapping.Select(mapping => mapping.EmployeeId).ToList();
            var employees = (await _unitOfWork.Employees.GetAll())
                .Where(emp => manageids.Contains(emp.Id))
                .Select(emp => new EmployeeRequestModel
                {
                    Id = emp.Id,
                    ManagerName = emp.FirstName + " " + emp.LastName,
                    ManagerId = emp.ManagerId,
                })
                .ToList();

            return employees;
        }

        public async Task<bool> ExistingEmployeeUpdate(EmployeeRequestModel employeeRequestModel)
        {
            var employee = await _unitOfWork.Employees.GetById(employeeRequestModel.Id);
            if (employee == null) return false;
            employee.SkillIds = employeeRequestModel.SkillIds;
            if (employeeRequestModel.ProjectIds != null)
            {
                employee.ProjectIds = employeeRequestModel.ProjectIds;
            }
            _unitOfWork.Employees.Update(employee);
            var result =_unitOfWork.Save();
            return result > 0;
        }

        public async Task<string?> UploadProfileImage(ProfileImageRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return null;

            // Security Hardening (SEC-21): Enforce max file size of 5 MB
            const long maxFileSize = 5 * 1024 * 1024;
            if (request.File.Length > maxFileSize)
                return null;

            // Strictly whitelist safe image extensions
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(request.File.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
                return null;

            var employee = await _unitOfWork.Employees.GetById(request.EmployeeId);

            if (employee == null)
                return null;

            if (!string.IsNullOrEmpty(employee.ProfileImage))
            {
                var oldFilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "ProfileImages",
                    Path.GetFileName(employee.ProfileImage)
                );

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "ProfileImages");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            employee.ProfileImage = fileName;

            _unitOfWork.Employees.Update(employee);
            _unitOfWork.Save();

            return employee.ProfileImage;
        }

        public async Task<IEnumerable<EmployeeRequestModel>> GetProbationRemindersAsync()
        {
            var allEmployees = await GetAllEmployees();
            var thresholdDate = DateTime.Today.AddDays(15);

            var reminders = allEmployees.Where(e =>
                string.Equals(e.ProbationStatus, "UnderProbation", StringComparison.OrdinalIgnoreCase) &&
                e.ProbationEndDate.HasValue &&
                e.ProbationEndDate.Value.Date <= thresholdDate.Date
            ).OrderBy(e => e.ProbationEndDate).ToList();

            return reminders;
        }

        public async Task<bool> ProcessProbationDecisionAsync(long employeeId, ProbationDecisionDto dto)
        {
            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null || employee.IsDeleted) throw new KeyNotFoundException($"Employee #{employeeId} not found.");

            var currentUserId = _userContextService?.GetCurrentEmployeeId() ?? 0;
            bool isAdminOrHr = _userContextService?.IsAdmin() == true || _userContextService?.IsHR() == true;
            bool isManager = employee.ManagerId.HasValue && employee.ManagerId.Value == currentUserId;

            if (!isAdminOrHr && !isManager)
            {
                throw new UnauthorizedAccessException("Only the reporting manager or HR can process probation decisions.");
            }

            var now = DateTime.Now;
            if (string.Equals(dto.Decision, "Confirm", StringComparison.OrdinalIgnoreCase))
            {
                employee.ProbationStatus = "Confirmed";
                employee.EmploymentStatus = "Active";
            }
            else if (string.Equals(dto.Decision, "Extend", StringComparison.OrdinalIgnoreCase))
            {
                if (!dto.NewEndDate.HasValue || dto.NewEndDate.Value.Date <= DateTime.Today)
                {
                    throw new InvalidOperationException("A valid future extension date is required to extend probation.");
                }
                employee.ProbationStatus = "Extended";
                employee.ProbationEndDate = dto.NewEndDate.Value;
            }
            else if (string.Equals(dto.Decision, "Terminate", StringComparison.OrdinalIgnoreCase))
            {
                employee.ProbationStatus = "Terminated";
                employee.EmploymentStatus = "NoticePeriod";
            }
            else
            {
                throw new ArgumentException($"Invalid probation decision '{dto.Decision}'. Allowed values: Confirm, Extend, Terminate.");
            }

            employee.UpdatedBy = (int)currentUserId;
            employee.UpdatedDate = now;
            _unitOfWork.Employees.Update(employee);

            return _unitOfWork.Save() > 0;
        }

        public async Task<bool> UpdateNoticeOverrideAsync(long employeeId, NoticePeriodOverrideDto dto)
        {
            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null || employee.IsDeleted) throw new KeyNotFoundException($"Employee #{employeeId} not found.");

            bool hasOverridePerm = _userContextService?.IsAdmin() == true ||
                                   _userContextService?.IsHR() == true ||
                                   await (_userContextService?.HasPermissionAsync("NOTICE_OVERRIDE") ?? Task.FromResult(false));

            if (!hasOverridePerm)
            {
                throw new UnauthorizedAccessException("Permission Denied: Overriding employee notice period requires the 'NOTICE_OVERRIDE' permission.");
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new InvalidOperationException("A mandatory justification reason is required to override notice period.");
            }

            var oldOverride = employee.NoticePeriodOverrideDays;
            employee.NoticePeriodOverrideDays = dto.NoticePeriodDays;
            employee.NoticePeriodOverrideReason = dto.Reason;
            employee.UpdatedBy = (int)(_userContextService?.GetCurrentEmployeeId() ?? 0);
            employee.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Employees.Update(employee);
            var saved = _unitOfWork.Save() > 0;

            if (saved && _auditService != null)
            {
                await _auditService.LogAsync("Workforce", "Employee", employeeId.ToString(), "NOTICE_OVERRIDE",
                    new { NoticePeriodOverrideDays = oldOverride },
                    new { NoticePeriodOverrideDays = dto.NoticePeriodDays, Reason = dto.Reason },
                    dto.Reason);
            }

            return saved;
        }

        public async Task<IEnumerable<MatchingFormerEmployeeDto>> GetMatchingFormerEmployeesAsync(string? email, string? pan, string? uan)
        {
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(pan) && string.IsNullOrWhiteSpace(uan))
            {
                return Enumerable.Empty<MatchingFormerEmployeeDto>();
            }

            var allEmployees = await _unitOfWork.Employees.GetAll();
            var matches = new List<MatchingFormerEmployeeDto>();

            var normalizedEmail = email?.Trim().ToLowerInvariant();
            var normalizedPan = pan?.Trim().ToUpperInvariant();
            var normalizedUan = uan?.Trim();

            foreach (var emp in allEmployees)
            {
                bool isExitedOrDeleted = emp.IsDeleted || string.Equals(emp.EmploymentStatus, "Exited", StringComparison.OrdinalIgnoreCase);
                if (!isExitedOrDeleted) continue;

                var reasons = new List<string>();

                if (!string.IsNullOrEmpty(normalizedPan) && !string.IsNullOrEmpty(emp.PanNumber) &&
                    string.Equals(emp.PanNumber.Trim(), normalizedPan, StringComparison.OrdinalIgnoreCase))
                {
                    reasons.Add("Matched by PAN");
                }

                if (!string.IsNullOrEmpty(normalizedUan) && !string.IsNullOrEmpty(emp.UanNumber) &&
                    string.Equals(emp.UanNumber.Trim(), normalizedUan, StringComparison.OrdinalIgnoreCase))
                {
                    reasons.Add("Matched by UAN");
                }

                if (!string.IsNullOrEmpty(normalizedEmail))
                {
                    if (!string.IsNullOrEmpty(emp.PersonalEmail) && string.Equals(emp.PersonalEmail.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        reasons.Add("Matched by Personal Email");
                    }
                    else if (!string.IsNullOrEmpty(emp.EmailAddress) && string.Equals(emp.EmailAddress.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        reasons.Add("Matched by Primary Email");
                    }
                }

                if (reasons.Any())
                {
                    matches.Add(new MatchingFormerEmployeeDto
                    {
                        EmployeeId = emp.Id,
                        FullName = $"{emp.FirstName} {emp.LastName}".Trim(),
                        EmailAddress = emp.EmailAddress ?? "",
                        PersonalEmail = emp.PersonalEmail,
                        PanNumber = emp.PanNumber,
                        UanNumber = emp.UanNumber,
                        Designation = emp.Designation,
                        DateOfJoining = emp.DateOfJoining,
                        ExitDate = emp.ExitDate,
                        EmploymentStatus = emp.EmploymentStatus ?? "Exited",
                        MatchReason = string.Join(", ", reasons)
                    });
                }
            }

            return matches;
        }

        public async Task<IEnumerable<HistoricalRehireReportItemDto>> GetHistoricalRehiresReportAsync()
        {
            var allEmployees = await _unitOfWork.Employees.GetAll();
            var rehireReport = allEmployees
                .Where(e => e.RehireDate.HasValue || e.PreviousEmployeeId.HasValue)
                .Select(e => new HistoricalRehireReportItemDto
                {
                    EmployeeId = e.Id,
                    FullName = $"{e.FirstName} {e.LastName}".Trim(),
                    EmailAddress = e.EmailAddress ?? "",
                    RehireDate = e.RehireDate,
                    CurrentDateOfJoining = e.DateOfJoining,
                    Designation = e.Designation ?? ""
                })
                .OrderByDescending(r => r.RehireDate ?? r.CurrentDateOfJoining)
                .ToList();

            return rehireReport;
        }

        public async Task<IEnumerable<PreviousEmploymentHistoryDto>> GetPreviousEmploymentHistoryAsync(long employeeId)
        {
            var history = new List<PreviousEmploymentHistoryDto>();
            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);

            if (!allEmployees.TryGetValue(employeeId, out var currentEmp)) return history;

            long? prevId = currentEmp.PreviousEmployeeId;
            var visited = new HashSet<long> { employeeId };

            var allPayslips = await _unitOfWork.Payslips.GetAll();
            var allDocs = await _unitOfWork.EmployeementDocument.GetAll();

            while (prevId.HasValue && allEmployees.TryGetValue(prevId.Value, out var prevEmp))
            {
                if (visited.Contains(prevId.Value)) break;
                visited.Add(prevId.Value);

                var payslipsForTenure = allPayslips
                    .Where(p => p.EmployeeId == prevEmp.Id && !p.IsDeleted)
                    .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                    .Select(p => new PreviousPayslipSummaryDto
                    {
                        Id = p.Id,
                        Month = p.Month,
                        Year = p.Year,
                        NetPay = p.NetSalary,
                        PayslipUrl = p.DocumentPath
                    }).ToList();

                var docsForTenure = allDocs
                    .Where(d => d.EmployeeId == prevEmp.Id && !d.IsDeleted)
                    .Select(d => new PreviousDocumentSummaryDto
                    {
                        Id = d.Id,
                        DocumentType = d.Category ?? "Document",
                        DocumentName = d.DocumentName ?? "File",
                        DocumentUrl = d.FilePath,
                        UploadedDate = d.UploadedDate ?? d.CreatedDate
                    }).ToList();

                history.Add(new PreviousEmploymentHistoryDto
                {
                    EmployeeId = prevEmp.Id,
                    EmployeeCode = prevEmp.EmployeeCode,
                    Designation = prevEmp.Designation ?? "",
                    DateOfJoining = prevEmp.DateOfJoining,
                    ExitDate = prevEmp.ExitDate,
                    EmploymentStatus = prevEmp.EmploymentStatus ?? "Exited",
                    Payslips = payslipsForTenure,
                    Documents = docsForTenure
                });

                prevId = prevEmp.PreviousEmployeeId;
            }

            return history;
        }
    }
}
