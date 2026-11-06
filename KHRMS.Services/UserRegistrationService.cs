using KHRMS.Core;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        public readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService? _tokenService;
        private readonly ISendEmailService? _sendEmailService;
        private readonly IMemoryCache? _memoryCache;
        private readonly IConfiguration? _configuration;

        public UserRegistrationService(
            IUnitOfWork unitOfWork,
            ITokenService? tokenService = null,
            ISendEmailService? sendEmailService = null,
            IMemoryCache? memoryCache = null,
            IConfiguration? configuration = null)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _sendEmailService = sendEmailService;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<bool> GetRegistrationByUser(UserRegistration userRegistration)
        {
            if (userRegistration == null || string.IsNullOrWhiteSpace(userRegistration.Email))
                return false;

            var emailTrimmed = userRegistration.Email.Trim();

            // Validate that email is unique across registrations, logins, and employees
            var allRegistrations = await _unitOfWork.UserRegistrations.GetAll();
            if (allRegistrations.Any(u => string.Equals(u.Email, emailTrimmed, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted))
            {
                return false;
            }

            var allLogins = await _unitOfWork.UserLogins.GetAll();
            if (allLogins.Any(u => string.Equals(u.Email, emailTrimmed, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted))
            {
                return false;
            }

            var allEmployees = await _unitOfWork.Employees.GetAll();
            if (allEmployees.Any(e => string.Equals(e.EmailAddress, emailTrimmed, StringComparison.OrdinalIgnoreCase) && !e.IsDeleted))
            {
                return false;
            }

            bool isUserRegistered = false;
            userRegistration.Email = emailTrimmed;
            userRegistration.CreatedDate = DateTime.Now;

            var passwordHasher = new PasswordHasher<UserRegistration>();
            userRegistration.Password = passwordHasher.HashPassword(userRegistration, userRegistration.Password ?? string.Empty);
            
            await _unitOfWork.UserRegistrations.Add(userRegistration);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                // 1. Create Employee first so we get the canonical Employee.Id
                var employee = await AddEmployeeFromRegistration(userRegistration);

                // 2. Link UserLogin directly to Employee.Id
                if (employee != null)
                {
                    isUserRegistered = await AddUserLogin(userRegistration, employee.Id);
                }
            }
            return isUserRegistered;
        }

        public async Task<InviteEmployeeResponse> InviteEmployeeAsync(InviteEmployeeRequest request, long creatorEmployeeId)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return new InviteEmployeeResponse
                {
                    Success = false,
                    Message = "Valid employee email is required."
                };
            }

            var emailTrimmed = request.Email.Trim();

            // 1. Check if an active login already exists
            var existingLogin = (await _unitOfWork.UserLogins.GetAll())
                .FirstOrDefault(u => string.Equals(u.Email, emailTrimmed, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted && u.IsActive);

            if (existingLogin != null)
            {
                return new InviteEmployeeResponse
                {
                    Success = false,
                    Message = "An active user login already exists for this email address."
                };
            }

            // 2. Find or create employee stub
            var allEmployees = await _unitOfWork.Employees.GetAll();
            Employee? employee = null;

            if (request.EmployeeId.HasValue && request.EmployeeId.Value > 0)
            {
                employee = allEmployees.FirstOrDefault(e => e.Id == request.EmployeeId.Value && !e.IsDeleted);
            }

            if (employee == null)
            {
                employee = allEmployees.FirstOrDefault(e => string.Equals(e.EmailAddress, emailTrimmed, StringComparison.OrdinalIgnoreCase) && !e.IsDeleted);
            }

            DateTime now = DateTime.UtcNow;

            if (employee == null)
            {
                employee = new Employee
                {
                    FirstName = !string.IsNullOrWhiteSpace(request.FirstName) ? request.FirstName.Trim() : "Invited",
                    LastName = !string.IsNullOrWhiteSpace(request.LastName) ? request.LastName.Trim() : "Employee",
                    EmailAddress = emailTrimmed,
                    PrimaryEmailAddress = emailTrimmed,
                    MobileNumber = request.MobileNumber?.Trim(),
                    CreatedDate = now,
                    DateOfJoining = now,
                    IsActive = true,
                    IsDeleted = false,
                    ProfileCompleted = false,
                    Branch = "Headquarters",
                    EmployeeCode = 0
                };

                await _unitOfWork.Employees.Add(employee);
                _unitOfWork.Save();

                employee.EmployeeCode = employee.Id;
                _unitOfWork.Employees.Update(employee);
                _unitOfWork.Save();
            }

            // Ensure base Employee role is mapped
            await EnsureEmployeeRoleMappedAsync(employee.Id);

            // 3. Generate Cryptographic Invitation Token
            if (_tokenService == null)
            {
                throw new InvalidOperationException("TokenService is required to generate invitation tokens.");
            }

            string token = _tokenService.GenerateInvitationToken(employee.EmailAddress, employee.Id, expiryHours: 48);

            // 4. Construct Invitation URL
            var clientBaseUrl = !string.IsNullOrWhiteSpace(request.ClientUrl) 
                ? request.ClientUrl 
                : (_configuration?["AppSettings:ClientUrl"] ?? "http://localhost:4200/#/register");

            var separator = clientBaseUrl.Contains('?') ? "&" : "?";
            var invitationUrl = $"{clientBaseUrl}{separator}token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(employee.EmailAddress)}";

            // 5. Send Email if service is configured
            bool emailSent = false;
            if (_sendEmailService != null)
            {
                try
                {
                    var subject = "You are invited to join the HRMS Portal";
                    var body = $"Hello {employee.FirstName},\n\nYou have been invited to set up your account on the HRMS Portal. " +
                               $"Please click the link below to complete your registration:\n\n{invitationUrl}\n\n" +
                               $"This invitation link is valid for 48 hours and can only be used once.\n\nThank you,\nHR Operations";

                    emailSent = await _sendEmailService.SendResetPasswordEmailAsync(employee.EmailAddress, subject, invitationUrl, "employee_invitation");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to dispatch invitation email to {Email}", employee.EmailAddress);
                }
            }

            Log.Information("UserRegistrationService - Generated employee invitation for {Email} (Emp #{EmployeeId}) by Creator #{CreatorId}", 
                employee.EmailAddress, employee.Id, creatorEmployeeId);

            return new InviteEmployeeResponse
            {
                Success = true,
                Message = "Employee invitation created successfully.",
                InvitationToken = token,
                InvitationUrl = invitationUrl,
                EmailSent = emailSent
            };
        }

        public async Task<VerifyInvitationResponse> VerifyInvitationTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new VerifyInvitationResponse
                {
                    IsValid = false,
                    Message = "Invitation token is required."
                };
            }

            if (_tokenService == null)
            {
                return new VerifyInvitationResponse { IsValid = false, Message = "Token service is unavailable." };
            }

            // Check single-use invalidation
            if (_memoryCache != null)
            {
                var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
                if (_memoryCache.TryGetValue($"used_invite_{tokenHash}", out _))
                {
                    return new VerifyInvitationResponse
                    {
                        IsValid = false,
                        Message = "This invitation link has already been used. Please log in with your credentials."
                    };
                }
            }

            var principal = _tokenService.ValidateInvitationToken(token);
            if (principal == null)
            {
                return new VerifyInvitationResponse
                {
                    IsValid = false,
                    Message = "Invalid or expired invitation token. Please request a new invitation link from HR."
                };
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;
            var empIdStr = principal.FindFirst("EmployeeId")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return new VerifyInvitationResponse
                {
                    IsValid = false,
                    Message = "Invitation token contains invalid identity claims."
                };
            }

            long.TryParse(empIdStr, out long empId);

            var existingEmployee = (await _unitOfWork.Employees.GetAll())
                .FirstOrDefault(e => (!e.IsDeleted) && (e.Id == empId || string.Equals(e.EmailAddress, email, StringComparison.OrdinalIgnoreCase)));

            var existingLogin = (await _unitOfWork.UserLogins.GetAll())
                .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted && u.IsActive);

            if (existingLogin != null)
            {
                return new VerifyInvitationResponse
                {
                    IsValid = false,
                    Message = "Registration has already been completed for this account. Please log in directly."
                };
            }

            return new VerifyInvitationResponse
            {
                IsValid = true,
                Message = "Invitation is valid.",
                Email = email,
                FirstName = existingEmployee?.FirstName,
                LastName = existingEmployee?.LastName,
                EmployeeId = existingEmployee?.Id
            };
        }

        public async Task<bool> AcceptInvitationAsync(AcceptInvitationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Password))
            {
                return false;
            }

            var verifyResult = await VerifyInvitationTokenAsync(request.Token);
            if (!verifyResult.IsValid || string.IsNullOrWhiteSpace(verifyResult.Email))
            {
                return false;
            }

            var emailTrimmed = verifyResult.Email.Trim();

            // Locate employee
            var allEmployees = await _unitOfWork.Employees.GetAll();
            var employee = allEmployees.FirstOrDefault(e => !e.IsDeleted &&
                (e.Id == verifyResult.EmployeeId || string.Equals(e.EmailAddress, emailTrimmed, StringComparison.OrdinalIgnoreCase)));

            DateTime now = DateTime.UtcNow;

            if (employee == null)
            {
                employee = new Employee
                {
                    FirstName = !string.IsNullOrWhiteSpace(request.FirstName) ? request.FirstName.Trim() : "Employee",
                    LastName = !string.IsNullOrWhiteSpace(request.LastName) ? request.LastName.Trim() : "",
                    EmailAddress = emailTrimmed,
                    PrimaryEmailAddress = emailTrimmed,
                    MobileNumber = request.MobileNumber?.Trim(),
                    CurrentAddress = request.Address?.Trim(),
                    PermanentAddress = request.Address?.Trim(),
                    DateOfJoining = now,
                    CreatedDate = now,
                    IsActive = true,
                    IsDeleted = false,
                    ProfileCompleted = true,
                    Branch = "Headquarters"
                };
                await _unitOfWork.Employees.Add(employee);
                _unitOfWork.Save();

                employee.EmployeeCode = employee.Id;
                _unitOfWork.Employees.Update(employee);
                _unitOfWork.Save();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(request.FirstName)) employee.FirstName = request.FirstName.Trim();
                if (!string.IsNullOrWhiteSpace(request.LastName)) employee.LastName = request.LastName.Trim();
                if (!string.IsNullOrWhiteSpace(request.MobileNumber)) employee.MobileNumber = request.MobileNumber.Trim();
                if (!string.IsNullOrWhiteSpace(request.Address))
                {
                    employee.CurrentAddress = request.Address.Trim();
                    employee.PermanentAddress = request.Address.Trim();
                }
                employee.ProfileCompleted = true;
                employee.UpdatedDate = now;
                _unitOfWork.Employees.Update(employee);
                _unitOfWork.Save();
            }

            // Create or update UserLogin
            var allLogins = await _unitOfWork.UserLogins.GetAll();
            var userLogin = allLogins.FirstOrDefault(u => string.Equals(u.Email, emailTrimmed, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted);

            var passwordHasher = new PasswordHasher<UserLogin>();
            var dummyLogin = new UserLogin { Email = emailTrimmed };
            string hashedPassword = passwordHasher.HashPassword(dummyLogin, request.Password);

            if (userLogin == null)
            {
                userLogin = new UserLogin
                {
                    UserId = employee.Id,
                    UserName = emailTrimmed,
                    Email = emailTrimmed,
                    Password = hashedPassword,
                    CreatedDate = now,
                    LastLoginDate = now,
                    IsActive = true,
                    IsDeleted = false,
                    IsResetPasswordRequired = false
                };
                await _unitOfWork.UserLogins.Add(userLogin);
            }
            else
            {
                userLogin.UserId = employee.Id;
                userLogin.Password = hashedPassword;
                userLogin.IsActive = true;
                userLogin.IsResetPasswordRequired = false;
                userLogin.UpdatedDate = now;
                _unitOfWork.UserLogins.Update(userLogin);
            }

            var saved = _unitOfWork.Save() > 0;

            if (saved)
            {
                await EnsureEmployeeRoleMappedAsync(employee.Id);

                // Invalidate single-use invitation token in cache
                if (_memoryCache != null)
                {
                    var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
                    _memoryCache.Set($"used_invite_{tokenHash}", true, TimeSpan.FromDays(7));
                }

                Log.Information("UserRegistrationService - Successfully completed invitation registration for {Email} (Emp #{EmployeeId})",
                    emailTrimmed, employee.Id);
            }

            return saved;
        }

        private async Task EnsureEmployeeRoleMappedAsync(long employeeId)
        {
            var allRoles = (await _unitOfWork.RoleMaster.GetAll())
                .Where(r => r.IsActive != false && r.IsDeleted != true)
                .ToList();

            var empRole = allRoles.FirstOrDefault(r => string.Equals(r.RoleName, "Employee", StringComparison.OrdinalIgnoreCase));
            if (empRole != null)
            {
                var roleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                    .Where(m => m.EmployeeId == employeeId && m.IsActive != false)
                    .ToList();

                if (!roleMappings.Any(m => m.RoleId == empRole.Id))
                {
                    await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                    {
                        EmployeeId = employeeId,
                        RoleId = empRole.Id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                    _unitOfWork.Save();
                }
            }
        }

        public async Task<bool> AddUserLogin(UserRegistration userRegistration, long employeeId)
        {
            UserLogin userLogin = new()
            {
                UserId = employeeId,
                UserName = userRegistration.Email ?? string.Empty,
                Email = userRegistration.Email ?? string.Empty,
                Password = userRegistration.Password ?? string.Empty,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                IsResetPasswordRequired = false
            };

            await _unitOfWork.UserLogins.Add(userLogin);
            var result = _unitOfWork.Save();
            return result > 0;
        }

        public async Task<Employee?> AddEmployeeFromRegistration(UserRegistration userRegistration)
        {
            Employee employee = new()
            {
                FirstName = userRegistration.FirstName,
                LastName = userRegistration.LastName,
                EmailAddress = userRegistration.Email,
                MobileNumber = userRegistration.MobileNumber,
                CurrentAddress = userRegistration.Address,
                PermanentAddress = userRegistration.Address,
                CreatedDate = DateTime.Now,
                DateOfJoining = DateTime.Now,
                DesignationId = 0,
                Gender = "",
                ManagerId = 0,
                ShiftIds = "",
                EmployeeCode = userRegistration.Id,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.Employees.Add(employee);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                // Ensure base roles exist
                var allRoles = (await _unitOfWork.RoleMaster.GetAll())
                    .Where(r => r.IsActive != false && r.IsDeleted != true)
                    .ToList();

                var baseRoles = new[] { "Admin", "HR", "Manager", "Employee" };
                bool rolesAdded = false;
                foreach (var baseRole in baseRoles)
                {
                    if (!allRoles.Any(r => string.Equals(r.RoleName, baseRole, StringComparison.OrdinalIgnoreCase)))
                    {
                        await _unitOfWork.RoleMaster.Add(new RoleMaster
                        {
                            RoleName = baseRole,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedDate = DateTime.UtcNow
                        });
                        rolesAdded = true;
                    }
                }
                if (rolesAdded)
                {
                    _unitOfWork.Save();
                    allRoles = (await _unitOfWork.RoleMaster.GetAll())
                        .Where(r => r.IsActive != false && r.IsDeleted != true)
                        .ToList();
                }

                var allRoleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                    .Where(r => r.IsActive != false)
                    .ToList();

                var adminRole = allRoles.FirstOrDefault(r => string.Equals(r.RoleName, "Admin", StringComparison.OrdinalIgnoreCase));
                var employeeRole = allRoles.FirstOrDefault(r => string.Equals(r.RoleName, "Employee", StringComparison.OrdinalIgnoreCase));

                bool systemHasAdmin = adminRole != null && allRoleMappings.Any(m => m.RoleId == adminRole.Id);
                var targetRole = (!systemHasAdmin && adminRole != null) ? adminRole : employeeRole;

                if (targetRole != null)
                {
                    await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                    {
                        EmployeeId = employee.Id,
                        RoleId = targetRole.Id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                    _unitOfWork.Save();
                }
            }

            return result > 0 ? employee : null;
        }
    }
}
