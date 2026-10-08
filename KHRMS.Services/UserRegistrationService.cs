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
            userRegistration.Password = passwordHasher.HashPassword(userRegistration, userRegistration.Password);
            
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
       

        public async Task<bool> AddUserLogin(UserRegistration userRegistration, long employeeId)
        {
            UserLogin userLogin = new()
            {
                UserId = employeeId,
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

            return result > 0 ? employee : null;
        }
    }
}
