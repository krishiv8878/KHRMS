using KHRMS.Core;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IUserLoginService
    {
        //Task<bool> CreateUserLogin(UserLogin userLogin);
        //Task<IEnumerable<UserLogin>> GetAllUserLogin();
      //  Task<bool> GetUserLoginById(string email,string Password);

        /* Task<bool> UpdateUserLogin (UserLogin userLogin);*/

        //Task<bool> DeleteUserLogin(long userLoginId);

        Task<UserLoginModel?> GetUserLoginById(string Email, string Password);

        Task<bool> ResetPassword(ResetPasswordModel userLogin);
        Task<bool> ForgotPasswordMail(ForgotPasswordRequestModel forgotPassword);

    }
}
