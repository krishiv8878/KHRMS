using KHRMS.Core;
using Microsoft.AspNetCore.Identity;


namespace KHRMS.Services
{
    public class UserRegistrationService(IUnitOfWork unitOfWork) : IUserRegistrationService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        //public async Task<bool> GetRegistrationByUser(UserRegistration userRegistration)
        //{
        //    bool isUserRegistered = false;
        //    if (userRegistration != null)
        //    {
        //        userRegistration.CreatedDate = DateTime.Now;
        //        var passwordHasher = new PasswordHasher<UserRegistration>();
        //        userRegistration.Password = passwordHasher.HashPassword(userRegistration,userRegistration.Password);
        //        await _unitOfWork.UserRegistrations.Add(userRegistration);
        //        var result = _unitOfWork.Save();

        //        if (result > 0)
        //        {
        //            isUserRegistered = await AddUserLogin(userRegistration);
        //        }
        //    }
        //    return isUserRegistered;
        //}
        public async Task<bool> GetRegistrationByUser(UserRegistration userRegistration)
        {
            bool isUserRegistered = false;
            if (userRegistration != null)
            {
                userRegistration.CreatedDate = DateTime.Now;

                var passwordHasher = new PasswordHasher<UserRegistration>();
                userRegistration.Password = passwordHasher.HashPassword(userRegistration, userRegistration.Password);

                await _unitOfWork.UserRegistrations.Add(userRegistration);
                var result = _unitOfWork.Save();

                if (result > 0)
                {
                    // Add UserLogin
                    isUserRegistered = await AddUserLogin(userRegistration);

                    // Add Employee Record
                    if (isUserRegistered)
                    {
                        await AddEmployeeFromRegistration(userRegistration);
                    }
                }
            }
            return isUserRegistered;
        }
       

        public async Task<bool> AddUserLogin(UserRegistration userRegistration)
        {
            UserLogin userLogin = new()
            {
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
            return result > 0;
        }


        public async Task<bool> AddEmployeeFromRegistration(UserRegistration userRegistration)
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
                DesignationId = 0, // default
                Gender = "", // or pass if available
                ManagerId = 0,
                ShiftIds = "",
                EmployeeCode = userRegistration.Id,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.Employees.Add(employee);
            var result = _unitOfWork.Save();
            return result > 0;
        }
    }
}
