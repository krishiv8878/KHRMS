using KHRMS.Core;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Identity; // Add this for PasswordHasher

namespace KHRMS.Services
{
    public class EmployeeService(IUnitOfWork unitOfWork, ISendEmailService emailRepository) : IEmployeeService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public ISendEmailService _sendEmailService = emailRepository;

        //public async Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel)
        //{
        //    if (employeeRequestModel == null)
        //        return false;
        //    // Find the most senior employee to assign as a manager
        //    var seniorEmployee = (await _unitOfWork.Employees.GetAll())
        //        .OrderBy(emp => emp.DateOfJoining)  // Oldest employee (seniority)
        //        .FirstOrDefault();
        //    var newEmployee = new Employee
        //    {
        //        EmployeeCode = employeeRequestModel.EmployeeCode,
        //        FirstName = employeeRequestModel.FirstName,
        //        LastName = employeeRequestModel.LastName,
        //        EmailAddress = employeeRequestModel.EmailAddress,
        //        MobileNumber = employeeRequestModel.MobileNumber,
        //        DesignationId = employeeRequestModel.DesignationId,
        //        DateOfJoining = employeeRequestModel.DateOfJoining,
        //        Gender = employeeRequestModel.Gender,
        //        CurrentAddress = employeeRequestModel.CurrentAddress,
        //        PermanentAddress = employeeRequestModel.PermanentAddress,
        //        IsActive = employeeRequestModel.IsActive,
        //        CreatedDate = DateTime.Now,
        //        ShiftIds = employeeRequestModel.ShiftId,
        //    };
        //    await _unitOfWork.Employees.Add(newEmployee);
        //    var result = _unitOfWork.Save();
        //    if (result > 0 && employeeRequestModel.RoleIds?.Any() == true)
        //    {
        //        var roleMappings = employeeRequestModel.RoleIds.Select(roleId => new EmployeeRoleMapping
        //        {
        //            EmployeeId = newEmployee.Id,
        //            RoleId = roleId,
        //            IsActive = true,
        //            CreatedDate = DateTime.Now
        //        }).ToList();

        //        foreach (var roleMapping in roleMappings)
        //        {
        //            await _unitOfWork.EmployeeRoleMappings.Add(roleMapping);
        //        }
        //        _unitOfWork.Save();
        //    }
        //    return result > 0;
        //}

        //public async Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel)
        //{
        //    if (employeeRequestModel == null)
        //        return false;

        //    var seniorEmployee = (await _unitOfWork.Employees.GetAll())
        //        .OrderBy(emp => emp.DateOfJoining)
        //        .FirstOrDefault();

        //    var newEmployee = new Employee
        //    {
        //        EmployeeCode = employeeRequestModel.EmployeeCode,
        //        FirstName = employeeRequestModel.FirstName,
        //        LastName = employeeRequestModel.LastName,
        //        EmailAddress = employeeRequestModel.EmailAddress,
        //        MobileNumber = employeeRequestModel.MobileNumber,
        //        PrimaryEmailAddress = employeeRequestModel.PrimaryEmailAddress, 
        //        DesignationId = employeeRequestModel.DesignationId,
        //        DateOfJoining = employeeRequestModel.DateOfJoining,
        //        Gender = employeeRequestModel.Gender,
        //        CurrentAddress = employeeRequestModel.CurrentAddress,
        //        PermanentAddress = employeeRequestModel.PermanentAddress,
        //        IsActive = employeeRequestModel.IsActive,
        //        CreatedDate = DateTime.Now,
        //        ShiftIds = employeeRequestModel.ShiftId,
        //    };

        //    await _unitOfWork.Employees.Add(newEmployee);
        //    var result = _unitOfWork.Save();


        //    // Add UserRegistration
        //    var defaultPassword = "System@123";
        //    var passwordHasher = new PasswordHasher<UserRegistration>();

        //    var userRegistration = new UserRegistration
        //    {
        //        FirstName = newEmployee.FirstName,
        //        LastName = newEmployee.LastName,
        //        Email = newEmployee.EmailAddress,
        //        MobileNumber = newEmployee.MobileNumber,
        //        Address = newEmployee.CurrentAddress,
        //        Password = passwordHasher.HashPassword(null, defaultPassword),
        //        CreatedDate = DateTime.Now
        //    };

        //    await _unitOfWork.UserRegistrations.Add(userRegistration);
        //    var regResult = _unitOfWork.Save();

        //    // Add UserLogin
        //    if (regResult > 0)
        //    {
        //        var userLogin = new KHRMS.Core.UserLogin
        //        {
        //            UserId = userRegistration.Id,
        //            UserName = userRegistration.Email,
        //            Email = userRegistration.Email,
        //            Password = userRegistration.Password,
        //            CreatedDate = DateTime.Now,
        //            IsActive = true,
        //            IsDeleted = false
        //        };

        //        await _unitOfWork.UserLogins.Add(userLogin);
        //        _unitOfWork.Save();
        //    }

        //    return result > 0;
        //}
        public async Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel)
        {
            if (employeeRequestModel == null)
                return false;

            var seniorEmployee = (await _unitOfWork.Employees.GetAll())
                .OrderBy(emp => emp.DateOfJoining)
                .FirstOrDefault();

            var newEmployee = new Employee
            {
                EmployeeCode = employeeRequestModel.EmployeeCode,
                FirstName = employeeRequestModel.FirstName,
                LastName = employeeRequestModel.LastName,
                EmailAddress = employeeRequestModel.EmailAddress,
                MobileNumber = employeeRequestModel.MobileNumber,
                PrimaryEmailAddress = employeeRequestModel.PrimaryEmailAddress,
                DesignationId = employeeRequestModel.DesignationId,
                DateOfJoining = employeeRequestModel.DateOfJoining,
                Gender = employeeRequestModel.Gender,
                CurrentAddress = employeeRequestModel.CurrentAddress,
                PermanentAddress = employeeRequestModel.PermanentAddress,
                IsActive = employeeRequestModel.IsActive,
                CreatedDate = DateTime.Now,
                ShiftIds = employeeRequestModel.ShiftId,
                PrimaryContactName = employeeRequestModel.PrimaryContactName,
                PrimaryContactRelationship = employeeRequestModel.PrimaryContactRelationship,
                PrimaryContactPhone = employeeRequestModel.PrimaryContactPhone,
                PrimaryContactEmail = employeeRequestModel.PrimaryContactEmail,
                PrimaryContactAddress = employeeRequestModel.PrimaryContactAddress,
                SecondaryContactName = employeeRequestModel.SecondaryContactName,
                SecondaryContactRelationship = employeeRequestModel.SecondaryContactRelationship,
                SecondaryContactPhone = employeeRequestModel.SecondaryContactPhone,
                SecondaryContactEmail = employeeRequestModel.SecondaryContactEmail,
                SecondaryContactAddress = employeeRequestModel.SecondaryContactAddress,
                Degree = employeeRequestModel.Degree,
                University = employeeRequestModel.University,
                YearOfPassing = employeeRequestModel.YearOfPassing,
                Percentage = employeeRequestModel.Percentage,
                CompanyName = employeeRequestModel.CompanyName,
                Designation = employeeRequestModel.Designation,
                ExperienceDuration = employeeRequestModel.ExperienceDuration,
                ExperienceLocation = employeeRequestModel.ExperienceLocation,
                Responsibilities = employeeRequestModel.Responsibilities,
                PassportNumber = employeeRequestModel.PassportNumber,
                Nationality = employeeRequestModel.Nationality,
                PassportIssueDate = employeeRequestModel.PassportIssueDate,
                PassportExpiryDate = employeeRequestModel.PassportExpiryDate,
                PassportScanCopy = employeeRequestModel.PassportScanCopy,
                Branch = employeeRequestModel.Branch,
                DateOfBirth = employeeRequestModel.DateOfBirth

            };

            await _unitOfWork.Employees.Add(newEmployee);
            var result = _unitOfWork.Save();

            var defaultPassword = "System@123";
            var passwordHasher = new PasswordHasher<UserRegistration>();
            var userRegistration = new UserRegistration
            {
                FirstName = newEmployee.FirstName,
                LastName = newEmployee.LastName,
                Email = newEmployee.EmailAddress,
                MobileNumber = newEmployee.MobileNumber,
                Address = newEmployee.CurrentAddress,
                Password = passwordHasher.HashPassword(null, defaultPassword),
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.UserRegistrations.Add(userRegistration);
            var regResult = _unitOfWork.Save();

            if (regResult > 0)
            {
                var userLogin = new KHRMS.Core.UserLogin
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
                _unitOfWork.Save();
            }

            return result > 0;
        }
    
        public async Task<bool> DeleteEmployee(long employeeId)
        {
            if (employeeId > 0)
            {
                var employeeDetails = await _unitOfWork.Employees.GetById(employeeId);
                if (employeeDetails != null)
                {
                    employeeDetails.IsDeleted = true;
                    employeeDetails.IsActive = false;

                    _unitOfWork.Employees.Update(employeeDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<EmployeeRequestModel>> GetAllEmployees()
        {
            var employees = await _unitOfWork.Employees.GetAll();
            var employeeroleMapping = await _unitOfWork.EmployeeRoleMappings.GetAll();
            var rolemaster = (await _unitOfWork.RoleMaster.GetAll()).ToDictionary(role => role.Id);
            var employeeDictionary = employees.ToDictionary(emp => emp.Id);

            var employeesWithRoles = employees.Select(emp => new EmployeeRequestModel
            {
                Id = emp.Id,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                EmailAddress = emp.EmailAddress,
                EmployeeCode = emp.EmployeeCode,
                MobileNumber = emp.MobileNumber,
                DesignationId = emp.DesignationId,
                DateOfJoining = emp.DateOfJoining,
                Gender = emp.Gender,
                CurrentAddress = emp.CurrentAddress,
                PermanentAddress = emp.PermanentAddress,
                IsActive = emp.IsActive,
                ManagerId = emp.ManagerId,
                CreatedDate = emp.CreatedDate,
                ShiftId = emp.ShiftIds,
                RoleIds = employeeroleMapping
                    .Where(mapping => mapping.EmployeeId == emp.Id && mapping.IsActive)
                    .Select(mapping => mapping.RoleId)
                    .Where(roleId => rolemaster.ContainsKey(roleId))
                    .ToList(),
                rolenames = employeeroleMapping
                    .Where(mapping => mapping.EmployeeId == emp.Id && mapping.IsActive)
                    .Select(mapping => mapping.RoleId)
                    .Where(roleId => rolemaster.ContainsKey(roleId))
                    .Select(roleId => rolemaster[roleId].RoleName)
                    .ToList(),
                ManagerName = employeeDictionary.ContainsKey(emp.ManagerId) ? $"{employeeDictionary[emp.ManagerId].FirstName} {employeeDictionary[emp.ManagerId].LastName}" : "Manager Not Assigned"

            }).OrderByDescending(emp => emp.CreatedDate) // Sort new entries at the top
    .ToList();



            return employeesWithRoles;
        }

        public async Task<Employee> GetEmployeeById(int employeeId)
        {
            if (employeeId > 0)
            {
                var employeeDetails = await _unitOfWork.Employees.GetById(employeeId);
                if (employeeDetails != null)
                {
                    return employeeDetails;
                }
            }
            return null;
        }

        public async Task<bool> UpdateEmployee(EmployeeRequestModel employeeRequestModel)
        {
            if (employeeRequestModel == null)
                return false;

            var employeeDetails = await _unitOfWork.Employees.GetById(employeeRequestModel.Id);
            if (employeeDetails == null)
                return false;

            // Update Employee details
            employeeDetails.EmployeeCode = employeeRequestModel.EmployeeCode;
            employeeDetails.FirstName = employeeRequestModel.FirstName;
            employeeDetails.LastName = employeeRequestModel.LastName;
            employeeDetails.EmailAddress = employeeRequestModel.EmailAddress;
            employeeDetails.MobileNumber = employeeRequestModel.MobileNumber;
            employeeDetails.DesignationId = employeeRequestModel.DesignationId;
            employeeDetails.DateOfJoining = employeeRequestModel.DateOfJoining;
            employeeDetails.Gender = employeeRequestModel.Gender;
            employeeDetails.CurrentAddress = employeeRequestModel.CurrentAddress;
            employeeDetails.PermanentAddress = employeeRequestModel.PermanentAddress;
            employeeDetails.IsActive = employeeRequestModel.IsActive;
            employeeDetails.UpdatedDate = DateTime.Now;
            employeeDetails.ShiftIds = employeeRequestModel.ShiftId;
            employeeDetails.ManagerId = employeeRequestModel.ManagerId;
            _unitOfWork.Employees.Update(employeeDetails);
            var saveEmployeeResult = _unitOfWork.Save();

            var existingRoleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(r => r.EmployeeId == employeeDetails.Id)
                .ToList();
            foreach (var roleMapping in existingRoleMappings.Where(r => r.IsActive))
            {
                roleMapping.IsActive = false;
                roleMapping.UpdatedDate = DateTime.Now;
                _unitOfWork.EmployeeRoleMappings.Update(roleMapping);
            }

            foreach (var roleId in employeeRequestModel.RoleIds)
            {
                var existingMapping = existingRoleMappings.FirstOrDefault(r => r.RoleId == roleId);

                if (existingMapping != null)
                {
                    existingMapping.IsActive = true;
                    existingMapping.UpdatedDate = DateTime.Now;
                    _unitOfWork.EmployeeRoleMappings.Update(existingMapping);
                }
                else
                {
                    await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                    {
                        RoleId = roleId,
                        EmployeeId = employeeDetails.Id,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    });
                }
            }
            var saveRoleMappingsResult = _unitOfWork.Save();
            return saveEmployeeResult > 0 && saveRoleMappingsResult > 0;
        }


        public async Task<IEnumerable<EmployeeRequestModel>> GetAllManagers()
        {
            var managerRole = (await _unitOfWork.RoleMaster.GetAll()).FirstOrDefault(r => r.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase));
            if (managerRole == null)
                return Enumerable.Empty<EmployeeRequestModel>();
            var employeeRoleMapping = (await _unitOfWork.EmployeeRoleMappings.GetAll()).Where(mapping => mapping.RoleId == managerRole.Id).ToList();
            var manageids = employeeRoleMapping.Select(mapping => mapping.EmployeeId).ToList();
            var employees = (await _unitOfWork.Employees.GetAll())
                .Where(emp => manageids.Contains(emp.Id))
                .Select(emp => new EmployeeRequestModel
                {
                    Id = emp.Id,
                    ManagerName = emp.FirstName + " " + emp.LastName,
                    ManagerId = emp.ManagerId,
                })
                .ToList();

            return employees;
        }

    }
}
