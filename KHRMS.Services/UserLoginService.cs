using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace KHRMS.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        public UserLoginService(IUnitOfWork unitOfWork,ISendEmailService sendEmail)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = sendEmail;
        
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

        public async Task<long?> GetUserLoginById(string email, string password)
        {
            var allUsers = await _unitOfWork.UserLogins.GetAll();
            var matchedUser = allUsers.FirstOrDefault(x => x.Email == email && !x.IsDeleted && x.IsActive);

            if (matchedUser != null)
            {
                var passwordHasher = new PasswordHasher<Core.UserLogin>();
                var verificationResult = passwordHasher.VerifyHashedPassword(matchedUser, matchedUser.Password, password);

                if (verificationResult == PasswordVerificationResult.Success)
                {
                    // Find matching employee
                    var allEmployees = await _unitOfWork.Employees.GetAll();
                    var matchedEmployee = allEmployees.FirstOrDefault(e => e.EmailAddress == email && !e.IsDeleted && e.IsActive);

                    if (matchedEmployee != null)
                    {
                        return matchedEmployee.Id;
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
                    var dict = new Dictionary<string, string>
                    {
                        {"Email" ,forgotPassword.Email}
                    };
                    var req = QueryHelpers.AddQueryString(forgotPassword.ClientUrl!, dict);

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
        public async Task<bool> ResetPassword(UserLoginModel userLogin)
        {
            if (userLogin != null)
            {
                var matchedUser = (await _unitOfWork.UserLogins.GetAll()).FirstOrDefault(x => x.Email == userLogin.Email && !x.IsDeleted && x.IsActive);
                if (matchedUser != null)
                {
                    var passwordHasher = new PasswordHasher<UserLogin>();
                    matchedUser.Password = passwordHasher.HashPassword(matchedUser,userLogin.Password);

                    _unitOfWork.UserLogins.Update(matchedUser);
                    return _unitOfWork.Save() > 0;
                }
            }
            return false;

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
