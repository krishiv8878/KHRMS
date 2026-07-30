using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using KHRMS.Core;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata; // Add this for PasswordHasher

namespace KHRMS.Services
{
    public class EmployeeService(IUnitOfWork unitOfWork, ISendEmailService emailRepository, ITokenService tokenService) : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ISendEmailService _sendEmailService = emailRepository;
        private readonly ITokenService _tokenService = tokenService;
        private const string AllowdChar = "abcdefghijklmnopqrstuvwxyz" +
                                          "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                          "0123456789" +
                                          "@#";
        private string GenerateRandomPassword(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            string password = RandomNumberGenerator.GetString(AllowdChar, length);
            return password;
        }
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

            // Get senior employee (best possible with current repo)
            var seniorEmployee = (await _unitOfWork.Employees.GetAll())
                .MinBy(e => e.DateOfJoining);

            var now = DateTime.Now;

            var newEmployee = new Employee
            {
                FirstName = employeeRequestModel.FirstName,
                LastName = employeeRequestModel.LastName,
                EmailAddress = employeeRequestModel.EmailAddress,
                DateOfJoining = employeeRequestModel.DateOfJoining,
                SkillIds = employeeRequestModel.SkillIds,
                //Designation = employeeRequestModel.Designation,
                DesignationId = 0,
                ManagerId = (long)employeeRequestModel.ManagerId,
                //Branch = employeeRequestModel.Branch,
                //Responsibilities = employeeRequestModel.Responsibilities,
                CreatedDate = now,
                CreatedBy = employeeRequestModel.CreatedBy,
                IsActive = employeeRequestModel.IsActive,
                IsDeleted = false,
                EmployeeCode = 0,
                ProfileCompleted = false
            };

            await _unitOfWork.Employees.Add(newEmployee);
            var res = _unitOfWork.Save();

            // Role mappings
            if (res > 0 && employeeRequestModel.RoleIds?.Any() == true)
            {
                foreach (var roleId in employeeRequestModel.RoleIds)
                {
                    await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                    {
                        EmployeeId = newEmployee.Id,
                        RoleId = roleId,
                        IsActive = true,
                        CreatedDate = now
                    });
                }
            }

            // Save employee + roles together
            var result = _unitOfWork.Save();
            if (result <= 0)
                return false;

            // User registration
            var defaultPassword = GenerateRandomPassword(12);
            var passwordHasher = new PasswordHasher<UserRegistration>();
            Console.WriteLine(defaultPassword);

            //var userRegistration = new UserRegistration
            //{
            //    FirstName = newEmployee.FirstName,
            //    LastName = newEmployee.LastName,
            //    Email = newEmployee.EmailAddress,
            //    MobileNumber = 0000000000,
            //    Address = "",
            //    Password = passwordHasher.HashPassword(null, defaultPassword),
            //    CreatedDate = now
            //};

            //await _unitOfWork.UserRegistrations.Add(userRegistration);

            // User login
            var userLogin = new UserLogin
            {
                UserId = newEmployee.Id,
                UserName = newEmployee.EmailAddress,
                Email = newEmployee.EmailAddress,
                Password = passwordHasher.HashPassword(null, defaultPassword),
                CreatedDate = now,
                IsActive = true,
                IsDeleted = false,
                IsResetPasswordRequired = true
            };

            await _unitOfWork.UserLogins.Add(userLogin);
            if(_unitOfWork.Save() > 0)
            {
                var CompanyName = "Krishiv Innovations";
                var TeamName = "HR";
                var token = _tokenService.GeneratePasswordResetToken(userLogin.Email);
                var SupportEmail = "hr@krishivinnovations.com";
                    var url = new Dictionary<string, string>
                    {
                        {"Token",token },
                        {"Email" ,userLogin.Email}
                    };
                    var req = QueryHelpers.AddQueryString(employeeRequestModel.ClientUrl!, url);
                    var dict = new Dictionary<string, string>
                    {
                        {"COMPANY_NAME",CompanyName},
                        {"USERNAME",userLogin.Email },
                        {"TEMP_PASSWORD" ,defaultPassword},
                        {"URL" , req},
                        {"SUPPORT_EMAIL" ,SupportEmail},
                        {"HR_OR_IT_TEAM_NAME" ,TeamName},
                    };
                    var subject = $"Welcome to {CompanyName} – Your Account Access Details";

                    await _sendEmailService.SendTemplateEmailAsync(userLogin.Email, subject, dict, "employee_onboarding_credentials");
                    return true;
            }

            // Save user-related entities together
            return _unitOfWork.Save() > 0;

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
            var allSkillIds = employees
                .Where(e => e.SkillIds != null)
                .SelectMany(e => e.SkillIds)
                .Distinct()
                .ToList();
            var skills = (await _unitOfWork.Skills.GetAll()).Where(s=>allSkillIds.Contains(s.Id)).ToDictionary(s=>s.Id, s=>s.SkillName);
            var rolemaster = (await _unitOfWork.RoleMaster.GetAll()).ToDictionary(role => role.Id);
            var employeeDictionary = employees.ToDictionary(emp => emp.Id);

            var employeesWithRoles = employees.Select(emp => new EmployeeRequestModel
            {
                Id = emp.Id,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                ProfileImage = emp.ProfileImage,
                EmailAddress = emp.EmailAddress,
                EmployeeCode = emp.EmployeeCode,
                MobileNumber = emp.MobileNumber,
                DesignationId =emp.DesignationId,
                DateOfJoining = emp.DateOfJoining,
                Gender = emp.Gender,
                CurrentAddress = emp.CurrentAddress,
                PermanentAddress = emp.PermanentAddress,
                IsActive = emp.IsActive,
                ManagerId = emp.ManagerId,
                CreatedDate = emp.CreatedDate,
                ShiftId = emp.ShiftIds,
                PrimaryEmailAddress = emp.PrimaryEmailAddress,
                PrimaryContactName = emp.PrimaryContactName,
                PrimaryContactRelationship = emp.PrimaryContactRelationship,
                PrimaryContactPhone = emp.PrimaryContactPhone,
                PrimaryContactEmail = emp.PrimaryContactEmail,
                PrimaryContactAddress = emp.PrimaryContactAddress,
                SecondaryContactName = emp.SecondaryContactName,
                SecondaryContactRelationship = emp.SecondaryContactRelationship,
                SecondaryContactPhone = emp.SecondaryContactPhone,
                SecondaryContactEmail = emp.SecondaryContactEmail,
                SecondaryContactAddress = emp.SecondaryContactAddress,
                Degree = emp.Degree,
                University = emp.University,
                YearOfPassing = emp.YearOfPassing,
                Percentage = emp.Percentage,
                CompanyName = emp.CompanyName,
                Designation = emp.Designation,
                ExperienceDuration = emp.ExperienceDuration,
                ExperienceLocation = emp.ExperienceLocation,
                Responsibilities = emp.Responsibilities,
                PassportNumber = emp.PassportNumber,
                Nationality = emp.Nationality,
                PassportIssueDate = emp.PassportIssueDate,
                PassportExpiryDate = emp.PassportExpiryDate,
                PassportScanCopy = emp.PassportScanCopy,
                Branch = emp.Branch,
                DateOfBirth = emp.DateOfBirth,
                SkillIds = emp.SkillIds ?? new List<long>(),
                Skills = (emp.SkillIds ?? Enumerable.Empty<long>())
                    .Where(id=>skills.ContainsKey(id))
                    .Select(id => skills[id]).ToList(),
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
            if (employeeDetails.ProfileCompleted == false)
            {
                employeeDetails.FirstName = employeeRequestModel.FirstName;
                employeeDetails.LastName = employeeRequestModel.LastName;
                employeeDetails.ProfileImage = employeeRequestModel.ProfileImage;
                employeeDetails.DateOfBirth = employeeRequestModel.DateOfBirth;
                employeeDetails.CurrentAddress = employeeRequestModel.CurrentAddress;
                employeeDetails.MobileNumber = employeeRequestModel.MobileNumber;
                employeeDetails.PermanentAddress = employeeRequestModel.PermanentAddress;
                employeeDetails.Gender = employeeRequestModel.Gender;
                employeeDetails.ProfileCompleted = (bool)employeeRequestModel.ProfileCompleted;
                _unitOfWork.Employees.Update(employeeDetails);
                var result = _unitOfWork.Save();
                return result > 0;
            }
            else
            {
                employeeDetails.EmployeeCode = employeeRequestModel.EmployeeCode;
                employeeDetails.FirstName = employeeRequestModel.FirstName;
                employeeDetails.LastName = employeeRequestModel.LastName;
                employeeDetails.ProfileImage = employeeRequestModel.ProfileImage;
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
                employeeDetails.ManagerId = (long)employeeRequestModel.ManagerId;
                employeeDetails.PrimaryEmailAddress = employeeRequestModel.PrimaryEmailAddress;
                employeeDetails.PrimaryContactName = employeeRequestModel.PrimaryContactName;
                employeeDetails.PrimaryContactRelationship = employeeRequestModel.PrimaryContactRelationship;
                employeeDetails.PrimaryContactPhone = employeeRequestModel.PrimaryContactPhone;
                employeeDetails.PrimaryContactEmail = employeeRequestModel.PrimaryContactEmail;
                employeeDetails.PrimaryContactAddress = employeeRequestModel.PrimaryContactAddress;
                employeeDetails.SecondaryContactName = employeeRequestModel.SecondaryContactName;
                employeeDetails.SecondaryContactRelationship = employeeRequestModel.SecondaryContactRelationship;
                employeeDetails.SecondaryContactPhone = employeeRequestModel.SecondaryContactPhone;
                employeeDetails.SecondaryContactEmail = employeeRequestModel.SecondaryContactEmail;
                employeeDetails.SecondaryContactAddress = employeeRequestModel.SecondaryContactAddress;
                employeeDetails.Degree = employeeRequestModel.Degree;
                employeeDetails.University = employeeRequestModel.University;
                employeeDetails.YearOfPassing = employeeRequestModel.YearOfPassing;
                employeeDetails.Percentage = employeeRequestModel.Percentage;
                employeeDetails.CompanyName = employeeRequestModel.CompanyName;
                employeeDetails.Designation = employeeRequestModel.Designation;
                employeeDetails.ExperienceDuration = employeeRequestModel.ExperienceDuration;
                employeeDetails.ExperienceLocation = employeeRequestModel.ExperienceLocation;
                employeeDetails.Responsibilities = employeeRequestModel.Responsibilities;
                employeeDetails.PassportNumber = employeeRequestModel.PassportNumber;
                employeeDetails.Nationality = employeeRequestModel.Nationality;
                employeeDetails.PassportIssueDate = employeeRequestModel.PassportIssueDate;
                employeeDetails.PassportExpiryDate = employeeRequestModel.PassportExpiryDate;
                employeeDetails.PassportScanCopy = employeeRequestModel.PassportScanCopy;
                employeeDetails.Branch = employeeRequestModel.Branch;
                employeeDetails.DateOfBirth = employeeRequestModel.DateOfBirth;
                employeeDetails.SkillIds = employeeRequestModel.SkillIds;
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

        public async Task<bool> ExistingEmployeeUpdate(EmployeeRequestModel employeeRequestModel)
        {
            var employee = await _unitOfWork.Employees.GetById(employeeRequestModel.Id);
            if (employee == null) return false;
            employee.SkillIds = employeeRequestModel.SkillIds;
            _unitOfWork.Employees.Update(employee);
            var result =_unitOfWork.Save();
            return result > 0;
        }

        public async Task<string?> UploadProfileImage(ProfileImageRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return null;

            var employee = await _unitOfWork.Employees.GetById(request.EmployeeId);

            if (employee == null)
                return null;

            // Delete old image if exists
            if (!string.IsNullOrEmpty(employee.ProfileImage))
            {
                var oldFilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    employee.ProfileImage.TrimStart('/')
                );

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            var fileName =
                Guid.NewGuid().ToString() +
                Path.GetExtension(request.File.FileName);

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "ProfileImages");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            employee.ProfileImage = fileName;

            _unitOfWork.Employees.Update(employee);
            _unitOfWork.Save();

            return employee.ProfileImage;
        }
    }
}
