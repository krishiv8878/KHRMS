using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KHRMS.Core;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
        public UserLoginService(IUnitOfWork unitOfWork,ISendEmailService sendEmail,ITokenService tokenService,IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = sendEmail;
            _tokenService = tokenService;
            _configuration = configuration;
        
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
            var allUsers = await _unitOfWork.UserLogins.GetAll();
            var matchedUser = allUsers.FirstOrDefault(x => x.Email == Email && !x.IsDeleted && x.IsActive);

            if (matchedUser != null)
            {
                var passwordHasher = new PasswordHasher<UserLogin>();
                var verificationResult = passwordHasher.VerifyHashedPassword(matchedUser, matchedUser.Password, Password);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    // Find matching employee
                    var allEmployees = await _unitOfWork.Employees.GetAll();
                    var matchedEmployee = allEmployees.FirstOrDefault(e => e.EmailAddress == Email && !e.IsDeleted && e.IsActive);
                    var existingRoleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
              .Where(r => r.EmployeeId == matchedEmployee.Id)
              .ToList().Select(x=> x.RoleId);

                    var allRole = (await _unitOfWork.RoleMaster.GetAll())
              .Where(r =>  r.IsActive != false && r.IsDeleted != true)
              .ToList();


                    var roleNames = allRole
                        .Where(r => existingRoleMappings.Contains(r.Id))
                        .Select(r => r.RoleName)   // Modify property name if yours is different
                        .ToList();


                    var roleTypes = "N/A";
                    if (roleNames.Count() > 0)
                    {
                        roleTypes = string.Join(", ", roleNames);
                    }
                    if (matchedEmployee != null)
                    {
                        var issuer = _configuration["Jwt:issuer"];
                        var audience = _configuration["Jwt:audience"];
                        var key = _configuration["Jwt:PasswordResetSecret"];
                        var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(60);

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {

                            Subject = new ClaimsIdentity(new[]
                            {
                                new Claim(JwtRegisteredClaimNames.Name, Email)
                            }),
                            Expires = tokenExpiryTimeStamp,
                            Issuer = issuer,
                            Audience = audience,
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
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
                            Password = Password,
                            Token = accessToken,
                            UserId = matchedEmployee.Id,
                            RoleType = roleTypes
                        };
                        return model;
                    }
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
                    var req = QueryHelpers.AddQueryString(forgotPassword.ClientUrl!,dict);

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

            _unitOfWork.UserLogins.Update(matchedUser);
            return _unitOfWork.Save() > 0;

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
             return null;
         }

         }*/
    }
}
