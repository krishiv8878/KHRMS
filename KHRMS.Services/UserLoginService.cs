using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KHRMS.Core;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KHRMS.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache? _memoryCache;

        public UserLoginService(
            IUnitOfWork unitOfWork,
            ISendEmailService sendEmail,
            ITokenService tokenService,
            IConfiguration configuration,
            IMemoryCache? memoryCache = null)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = sendEmail;
            _tokenService = tokenService;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }
        
        //public async Task<bool> GetUserLoginById(string email, string password)
        //{
        //    var allUsers = await _unitOfWork.UserLogins.GetAll();
        //    var matchedUser = allUsers.FirstOrDefault(x => x.Email == email && !x.IsDeleted && x.IsActive);
        //    if (matchedUser != null)
        //    {
        //        var passwordHasher = new PasswordHasher<UserLogin>();
        //        var verificationResult = passwordHasher.VerifyHashedPassword(matchedUser, matchedUser.Password, password);
        //        Console.WriteLine(verificationResult);

        //        return verificationResult == PasswordVerificationResult.Success;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public async Task<UserLoginModel?> GetUserLoginById(string Email, string Password)
        {
            var normalizedEmail = (Email ?? string.Empty).ToLowerInvariant().Trim();
            if (_memoryCache != null && !string.IsNullOrWhiteSpace(normalizedEmail))
            {
                if (_memoryCache.TryGetValue($"login_lockout_{normalizedEmail}", out DateTime lockoutUntil))
                {
                    var remaining = lockoutUntil.Subtract(DateTime.UtcNow);
                    if (remaining.TotalSeconds > 0)
                    {
                        var minutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
                        throw new InvalidOperationException($"Account is temporarily locked due to 5 consecutive failed login attempts. Please try again after {minutes} minutes or reset your password.");
                    }
                }
            }

            var allUsers = await _unitOfWork.UserLogins.GetAll();
            var matchedUser = allUsers.FirstOrDefault(x => x.Email == Email && !x.IsDeleted && x.IsActive);

            if (matchedUser != null)
            {
                var passwordHasher = new PasswordHasher<UserLogin>();
                var verificationResult = passwordHasher.VerifyHashedPassword(matchedUser, matchedUser.Password, Password);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    if (_memoryCache != null && !string.IsNullOrWhiteSpace(normalizedEmail))
                    {
                        _memoryCache.Remove($"login_failed_{normalizedEmail}");
                        _memoryCache.Remove($"login_lockout_{normalizedEmail}");
                    }
                    // Find matching employee
                    var allEmployees = await _unitOfWork.Employees.GetAll();
                    var existingEmpRecord = allEmployees.FirstOrDefault(e => string.Equals(e.EmailAddress, Email, StringComparison.OrdinalIgnoreCase));

                    // If employee exists but has been terminated/deactivated, deny login!
                    if (existingEmpRecord != null && (existingEmpRecord.IsDeleted || !existingEmpRecord.IsActive))
                    {
                        return null;
                    }

                    var matchedEmployee = existingEmpRecord != null && !existingEmpRecord.IsDeleted && existingEmpRecord.IsActive 
                        ? existingEmpRecord 
                        : null;

                    // If user exists in UserLogin but employee record is missing completely (initial system bootstrap only), auto-create it
                    if (matchedEmployee == null)
                    {
                        var newEmp = new Employee
                        {
                            FirstName = !string.IsNullOrWhiteSpace(matchedUser.UserName) ? matchedUser.UserName : "Admin",
                            LastName = "User",
                            EmailAddress = matchedUser.Email,
                            CreatedDate = DateTime.UtcNow,
                            DateOfJoining = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false,
                            ProfileCompleted = true
                        };
                        await _unitOfWork.Employees.Add(newEmp);
                        _unitOfWork.Save();
                        matchedEmployee = newEmp;
                    }

                    // Auto-seed base roles if RoleMaster is empty or missing standard roles
                    var allRole = (await _unitOfWork.RoleMaster.GetAll())
                        .Where(r => r.IsActive != false && r.IsDeleted != true)
                        .ToList();

                    var baseRoles = new[] { "Admin", "HR", "Manager", "Employee" };
                    bool rolesAdded = false;
                    foreach (var baseRole in baseRoles)
                    {
                        if (!allRole.Any(r => string.Equals(r.RoleName, baseRole, StringComparison.OrdinalIgnoreCase)))
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
                        allRole = (await _unitOfWork.RoleMaster.GetAll())
                            .Where(r => r.IsActive != false && r.IsDeleted != true)
                            .ToList();
                    }

                    var roleNames = new List<string>();
                    var existingRoleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                        .Where(r => r.EmployeeId == matchedEmployee.Id && r.IsActive != false)
                        .Select(x => x.RoleId)
                        .ToList();

                    roleNames = allRole
                        .Where(r => existingRoleMappings.Contains(r.Id) && !string.IsNullOrWhiteSpace(r.RoleName))
                        .Select(r => r.RoleName!)
                        .ToList();

                    // Check if ANY employee in the entire database has an Admin role mapped
                    var allRoleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                        .Where(r => r.IsActive != false)
                        .ToList();

                    var adminRole = allRole.FirstOrDefault(r => string.Equals(r.RoleName, "Admin", StringComparison.OrdinalIgnoreCase));
                    bool systemHasAdmin = adminRole != null && allRoleMappings.Any(m => m.RoleId == adminRole.Id);

                    // If the system has NO admin account yet (initial setup), make this logging-in user the Admin!
                    if (!systemHasAdmin && adminRole != null)
                    {
                        await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                        {
                            EmployeeId = matchedEmployee.Id,
                            RoleId = adminRole.Id,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        });
                        _unitOfWork.Save();

                        if (!roleNames.Contains("Admin"))
                        {
                            roleNames.Add("Admin");
                        }
                    }

                    if (roleNames.Count == 0)
                    {
                        roleNames.Add("Employee");
                    }

                    var roleTypes = string.Join(",", roleNames);

                    if (matchedEmployee != null)
                    {
                        var issuer = _configuration["Jwt:ValidIssuer"] ?? _configuration["Jwt:issuer"];
                        var audience = _configuration["Jwt:ValidAudience"] ?? _configuration["Jwt:audience"];
                        var key = !string.IsNullOrWhiteSpace(_configuration["Jwt:PasswordResetSecret"])
                            ? _configuration["Jwt:PasswordResetSecret"]!
                            : "DefaultSecretKeyForJwtTokenAuth12345";
                        var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(60); // SEC-01: Token lifetime shortened from 30 days to 60 minutes

                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Name, Email),
                            new Claim("UserId", matchedEmployee.Id.ToString()),
                            new Claim("userId", matchedEmployee.Id.ToString()),
                            new Claim(ClaimTypes.NameIdentifier, matchedEmployee.Id.ToString()),
                            new Claim(ClaimTypes.Email, Email),
                            new Claim("roleType", roleTypes)
                        };

                        foreach (var role in roleNames)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                            claims.Add(new Claim("role", role));
                        }

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(claims),
                            Expires = tokenExpiryTimeStamp,
                            Issuer = issuer,
                            Audience = audience,
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                                SecurityAlgorithms.HmacSha256),
                        };

                        var tokenHandler = new JwtSecurityTokenHandler();
                        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                        var accessToken = tokenHandler.WriteToken(securityToken);

                        var ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds;
                        var model = new UserLoginModel
                        {
                            UserName= $"{matchedEmployee.FirstName} {matchedEmployee.LastName}",
                            Email = Email,
                            Password = null, // Neutralized: Never echo plaintext password back in response body
                            Token = accessToken,
                            UserId = matchedEmployee.Id,
                            RoleType = roleTypes,
                            ProfileCompleted = matchedEmployee.ProfileCompleted,
                            IsResetPasswordRequired = matchedUser.IsResetPasswordRequired,
                        };
                        matchedUser.LastLoginDate = DateTime.UtcNow;
                        _unitOfWork.UserLogins.Update(matchedUser);
                        _unitOfWork.Save();
                        return model;
                    }
                }
            }
            // SEC-04: Failed login attempt counter & lockout logic
            if (_memoryCache != null && !string.IsNullOrWhiteSpace(normalizedEmail))
            {
                var failKey = $"login_failed_{normalizedEmail}";
                int failedAttempts = _memoryCache.TryGetValue(failKey, out int count) ? count + 1 : 1;
                if (failedAttempts >= 5)
                {
                    var lockUntil = DateTime.UtcNow.AddMinutes(15);
                    _memoryCache.Set($"login_lockout_{normalizedEmail}", lockUntil, TimeSpan.FromMinutes(15));
                    _memoryCache.Remove(failKey);
                    throw new InvalidOperationException("Account has been temporarily locked for 15 minutes due to 5 consecutive failed login attempts. Please try again later or reset your password.");
                }
                else
                {
                    _memoryCache.Set(failKey, failedAttempts, TimeSpan.FromMinutes(15));
                }
            }

            return null;
        }
        public async Task<bool> ForgotPasswordMail(ForgotPasswordRequestModel forgotPassword)
        {
            if (forgotPassword != null)
            {
                var userLoginDetails = (await _unitOfWork.UserLogins.GetAll()).FirstOrDefault(x => x.Email == forgotPassword.Email);
                if (userLoginDetails != null)
                {
                    var token = _tokenService.GeneratePasswordResetToken(forgotPassword.Email);
                    var dict = new Dictionary<string, string>
                    {
                        {"Token",token },
                        {"Email" ,forgotPassword.Email}
                    };
                    // Prevent password reset poisoning / open redirect: validate against configured baseUrl or localhost
                    var fallbackClientUrl = _configuration["AppSettings:ClientUrl"] ?? "http://localhost:4200/#/reset-password";
                    var targetBaseUrl = fallbackClientUrl;
                    if (!string.IsNullOrWhiteSpace(forgotPassword.ClientUrl) &&
                        Uri.TryCreate(forgotPassword.ClientUrl, UriKind.Absolute, out var parsedClientUri))
                    {
                        var configuredHost = Uri.TryCreate(fallbackClientUrl, UriKind.Absolute, out var parsedConfigUri) ? parsedConfigUri.Host : "localhost";
                        if (string.Equals(parsedClientUri.Host, configuredHost, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(parsedClientUri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                        {
                            targetBaseUrl = forgotPassword.ClientUrl;
                        }
                    }
                    var req = QueryHelpers.AddQueryString(targetBaseUrl, dict);

                    var subject = $"Reset Password For {userLoginDetails.UserName}";

                    await _sendEmailService.SendResetPasswordEmailAsync(forgotPassword.Email, subject, req, "ForgotPassword");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
        public async Task<bool> ResetPassword(ResetPasswordModel userLogin)
        {
            if (userLogin == null || string.IsNullOrWhiteSpace(userLogin.Token))
                return false;

            // SEC-03: Invalidate single-use password reset token
            if (_memoryCache != null)
            {
                var tokenHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(userLogin.Token)));
                var usedTokenKey = $"used_reset_token_{tokenHash}";
                if (_memoryCache.TryGetValue(usedTokenKey, out _))
                {
                    throw new InvalidOperationException("This password reset token has already been used. Please request a new password reset link.");
                }
            }

            var principal = _tokenService.ValidatePasswordResetToken(userLogin.Token);
            if (principal == null)
                return false;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var matchedUser = (await _unitOfWork.UserLogins.GetAll())
                .FirstOrDefault(x => x.Email == email && !x.IsDeleted && x.IsActive);

            if (matchedUser == null)
                return false;

            var passwordHasher = new PasswordHasher<UserLogin>();
            matchedUser.Password = passwordHasher.HashPassword(matchedUser, userLogin.Password);
            matchedUser.IsResetPasswordRequired = false;

            _unitOfWork.UserLogins.Update(matchedUser);
            var saved = _unitOfWork.Save() > 0;
            if (saved && _memoryCache != null)
            {
                var tokenHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(userLogin.Token)));
                _memoryCache.Set($"used_reset_token_{tokenHash}", true, TimeSpan.FromHours(24));
                var normalizedEmail = (email ?? string.Empty).ToLowerInvariant().Trim();
                _memoryCache.Remove($"login_lockout_{normalizedEmail}");
                _memoryCache.Remove($"login_failed_{normalizedEmail}");
            }
            return saved;
        }

        /* public async Task<bool> CreateUserLogin(UserLogin userLogin)
         {
             if(userLogin != null)
             {
                 userLogin.CreatedDate = DateTime.Now;
                 await _unitOfWork.UserLogins.Add(userLogin);

                 var result = _unitOfWork.Save();

                 if (result > 0)
                     return true;
                 else
                     return false;
             }
             return false;
         }

         public async Task<bool> DeleteUserLogin(long userLoginId)
         {
             if(userLoginId > 0)
             {
                 var userLoginDetails = await _unitOfWork.UserLogins.GetById(userLoginId);
                 if (userLoginDetails != null)
                 {
                     userLoginDetails.IsDeleted = true;
                     userLoginDetails.IsActive= false;

                     _unitOfWork.UserLogins.Update(userLoginDetails);

                     var result = _unitOfWork.Save();

                     if (result > 0)
                         return true;
                     else
                         return false;

                 }
             }
             return false;
         }

         public async Task<IEnumerable<UserLogin>> GetAllUserLogin()
         {
            var userLogins = await _unitOfWork.UserLogins.GetAll();
            return userLogins;
         }
 */
        /* public async Task<UserLogin> GetUserLoginById(int userLoginId)
         {
            if(userLoginId > 0)
             {
                 var userLoginDetails = await _unitOfWork.UserLogins.GetById(userLoginId);
                 if(userLoginDetails != null)
                 {
                     return userLoginDetails;
                 }
             }
             // SEC-04: Failed login attempt counter & lockout logic
            if (_memoryCache != null && !string.IsNullOrWhiteSpace(normalizedEmail))
            {
                var failKey = $"login_failed_{normalizedEmail}";
                int failedAttempts = _memoryCache.TryGetValue(failKey, out int count) ? count + 1 : 1;
                if (failedAttempts >= 5)
                {
                    var lockUntil = DateTime.UtcNow.AddMinutes(15);
                    _memoryCache.Set($"login_lockout_{normalizedEmail}", lockUntil, TimeSpan.FromMinutes(15));
                    _memoryCache.Remove(failKey);
                    throw new InvalidOperationException("Account has been temporarily locked for 15 minutes due to 5 consecutive failed login attempts. Please try again later or reset your password.");
                }
                else
                {
                    _memoryCache.Set(failKey, failedAttempts, TimeSpan.FromMinutes(15));
                }
            }

            return null;
         }

         }*/
    }
}
