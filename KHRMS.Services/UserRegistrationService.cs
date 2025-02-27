using KHRMS.Core;
using Microsoft.AspNetCore.Identity;


namespace KHRMS.Services
{
    public class UserRegistrationService(IUnitOfWork unitOfWork) : IUserRegistrationService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> GetRegistrationByUser(UserRegistration userRegistration)
        {
            bool isUserRegistered = false;
            if (userRegistration != null)
            {
                userRegistration.CreatedDate = DateTime.Now;
                var passwordHasher = new PasswordHasher<UserRegistration>();
                userRegistration.Password = passwordHasher.HashPassword(userRegistration,userRegistration.Password);
                await _unitOfWork.UserRegistrations.Add(userRegistration);
                var result = _unitOfWork.Save();

                if (result > 0)
                {
                    isUserRegistered = await AddUserLogin(userRegistration);
                }
            }
            return isUserRegistered;
        }

        public async Task<bool> AddUserLogin(UserRegistration userRegistration)
        {
            UserLogin userLogin = new()
            {
                // Id = userRegistration.Id,
                UserId = userRegistration.Id,
                UserName = userRegistration.Email,               
                Email = userRegistration.Email,
                Password = userRegistration.Password,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false

            };
            await _unitOfWork.UserLogins.Add(userLogin);
            var result = _unitOfWork.Save();
            if (result > 0)
                return true;
            else
                return false;
        }


    }
}
