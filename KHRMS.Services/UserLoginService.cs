using KHRMS.Core;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace KHRMS.Services
{
    public class UserLoginService(IUnitOfWork unitOfWork) : IUserLoginService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
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
                var passwordHasher = new PasswordHasher<UserLogin>();
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

         public async Task<bool> UpdateUserLogin(UserLogin userLogin)
         {
            if(userLogin != null)
             {
                 var userLoginDetails = await _unitOfWork.UserLogins.GetById(userLogin.Id);
                 if(userLoginDetails != null)
                 {
                     userLoginDetails.UserName = userLogin.UserName;
                     userLoginDetails.Password = userLogin.Password;
                     userLoginDetails.LastLoginDate = userLogin.LastLoginDate;


                     _unitOfWork.UserLogins.Update(userLoginDetails);
                     var result = _unitOfWork.Save();
                     if (result > 0)
                         return true;
                     else
                         return false;
                 }

             }
             return false;
         }*/
    }
}
