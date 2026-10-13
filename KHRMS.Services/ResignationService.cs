

using KHRMS.Core;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Engines;

namespace KHRMS.Services
{
    public class ResignationService : IResignationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        private readonly IUserContextService _userContextService;
        public ResignationService(IUnitOfWork unitOfWork, ISendEmailService sendEmailService, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = sendEmailService;
            _userContextService = userContextService;
        }
        public async Task<bool> AddResignations(ResignationRequestModel resignation)
        {
            long empid = _userContextService.GetCurrentEmployeeId();
            var employee = await _unitOfWork.Employees.GetById(empid);
            if (employee == null)
                throw new Exception("Employee not found.");

            Employee? manager = null;
            if (employee.ManagerId.HasValue && employee.ManagerId.Value > 0)
            {
                manager = (await _unitOfWork.Employees.GetAll())
                              .FirstOrDefault(t => t.Id == employee.ManagerId.Value);
            }
            if (manager == null)
            {
                // Fallback to HR or another administrator so submission does not crash
                manager = (await _unitOfWork.Employees.GetAll())
                              .FirstOrDefault(t => t.Id != empid && !string.IsNullOrEmpty(t.EmailAddress)) ?? employee;
            }

            var roles = (await _unitOfWork.EmployeeRoleMappings.GetAll()).Where(t => t.EmployeeId == empid);
            var roleid = roles.Select(r => r.RoleId).ToList();

            var newResign = new Resignation
            {
                EmployeeId = empid,
                Reason = resignation.Reason,
                Status = false,
                ManagerId = manager.Id,
                NoticePeriod = resignation.NoticePeriod,
                CreatedDate = DateTime.Now,
                Resignation_Date = DateTime.Now,
                RoleId = roleid,
            };
            await _unitOfWork.Resignation.Add(newResign);
            var result = _unitOfWork.Save();

            // Format dates for email
            try
            {
                string formattedStartDate = GetFormattedDate(newResign.Resignation_Date);
                var managerEmail = manager.EmailAddress;
                var managerName = $"{manager.FirstName} {manager.LastName}".Trim();
                var employeeName = $"{employee.FirstName} {employee.LastName}".Trim();
                var temptype = "ResignationRequest";

                var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "Resignation-Date", formattedStartDate },
                    { "NoticePeriod", newResign.NoticePeriod ?? "" },
                    { "EmployeeName", employeeName },
                    { "ManagerEmail", managerEmail ?? "" },
                    { "Reason", resignation.Reason ?? "" }
                };

                var subject = $"Resignation Request from {employeeName}";
                if (!string.IsNullOrEmpty(managerEmail))
                {
                    await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, temptype);
                }
            }
            catch
            {
                // Do not block submission on email failure
            }

            return result > 0;
        }
        private string GetFormattedDate(DateTime date)
        {
            int day = date.Day;
            string suffix = (day % 10 == 1 && day != 11) ? "st"
                         : (day % 10 == 2 && day != 12) ? "nd"
                         : (day % 10 == 3 && day != 13) ? "rd"
                         : "th";

            return $"{day}{suffix} {date:MMMM yyyy}";
        }

        public async Task<bool> ApproveOrRejectResignation(Resignation resignation)
        {
            if (resignation == null)
            {
                return false;
            }
            var res = await _unitOfWork.Resignation.GetById(resignation.Id);
            if (res == null)
            {
                return false;
            }

            var managerid = _userContextService.GetCurrentEmployeeId();
            var manager = (await _unitOfWork.Employees.GetAll())
                         .FirstOrDefault(t => t.Id == managerid);
            var employee = (await _unitOfWork.Employees.GetAll())
                         .FirstOrDefault(t => t.Id == res.EmployeeId);

            res.Status = resignation.Status;
            res.UpdatedDate = DateTime.Now;
            res.UpdatedBy = (int)managerid;

            string formattedStartDate = GetFormattedDate(res.Resignation_Date);
            var employeeEmail = employee?.EmailAddress ?? string.Empty;
            var managerName = manager != null ? $"{manager.FirstName} {manager.LastName}".Trim() : "Management";
            var employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : "Employee";
            if (res.Status == true)
            {
                //Format dates for email
                var temptype = "ResignationRequestApproved";

                // Create email placeholders
                var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "Resignation-Date", formattedStartDate },
                    { "NoticePeriod", res.NoticePeriod },
                    { "EmployeeName", employeeName },
                };

                var subject = $"Resignation Request from {employeeName} is Approved";

                await _sendEmailService.SendTemplateEmailAsync(employeeEmail, subject, dict, temptype);
                res.IsActive = false;
            }
            else
            {
                //Format dates for email
                
                var temptype = "ResignationRequestRejected";
                // Create email placeholders
                var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "Resignation-Date", formattedStartDate },
                    { "EmployeeName", employeeName },
                };

                var subject = $"Resignation Request from {employeeName} is Rejected";

                await _sendEmailService.SendTemplateEmailAsync(employeeEmail, subject, dict, temptype);
                res.IsActive = true;
            }
            _unitOfWork.Resignation.Update(res);
            var result = _unitOfWork.Save();

            return result > 0;
        }

        public async Task<bool> DeleteResignation(long ResignationId)
        {
            if (ResignationId <= 0) 
                return false;

            var resignation = await _unitOfWork.Resignation.GetById(ResignationId);
            if (resignation != null)
            {
                resignation.IsDeleted = true;
                resignation.IsActive = false;

                _unitOfWork.Resignation.Update(resignation);
                return _unitOfWork.Save() > 0;
            }

            return false;
        }

        public async Task<IEnumerable<ResignationRequestModel>> GetAllResignations()
        {
            var empid = _userContextService.GetCurrentEmployeeId();
            var resignations = (await _unitOfWork.Resignation.GetAll()).Where(x=>x.ManagerId == empid);
            var employees = (await _unitOfWork.Employees.GetAll()).Where(x=>x.ManagerId == empid);
            var roleMappings = await _unitOfWork.EmployeeRoleMappings.GetAll();
            var roles = (await _unitOfWork.RoleMaster.GetAll()).ToDictionary(r=>r.Id);
            var employeeDictionary = employees.ToDictionary(emp => emp.Id);


            var result = resignations.Select(p=> new ResignationRequestModel
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                Reason = p.Reason,
                Status = p.Status,
                ManagerId = p.ManagerId,
                NoticePeriod = p.NoticePeriod,
                Resignation_Date = p.Resignation_Date,
                RoleId = roleMappings
                    .Where(mapping => mapping.EmployeeId == p.EmployeeId && mapping.IsActive)
                    .Select(mapping => mapping.RoleId)
                    .Where(roleId => roles.ContainsKey(roleId))
                    .ToList(),
                RoleName = roleMappings
                    .Where(mapping => mapping.EmployeeId == p.EmployeeId && mapping.IsActive)
                    .Select(mapping => mapping.RoleId)
                    .Where(roleId => roles.ContainsKey(roleId))
                    .Select(roleId => roles[roleId].RoleName)
                    .ToList(),

            }).ToList();
            return result;

        }
        public async Task<Resignation> GetResignationById(int resignationId)
        {
            return await _unitOfWork.Resignation.GetByIdAsync(resignationId);
        }

        public async Task<bool> UpdateResignation(Resignation resignation)
        {
            if (resignation == null)
                return false;
            var resgin = await _unitOfWork.Resignation.GetById(resignation.Id);
            if (resgin == null)
                return false;

            resgin.Reason = resignation.Reason;
            resgin.NoticePeriod = resignation.NoticePeriod;
            resgin.UpdatedDate = DateTime.Now;

            _unitOfWork.Resignation.Update(resgin);
            var result = _unitOfWork.Save();

            return result > 0;
        }
    }
}
