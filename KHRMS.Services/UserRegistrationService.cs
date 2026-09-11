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
                IsDeleted = false,
                IsResetPasswordRequired = false
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

                // Check if any admin exists in the system
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

            return result > 0;
        }
    }
}
