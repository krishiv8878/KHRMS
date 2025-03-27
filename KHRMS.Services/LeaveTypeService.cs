using KHRMS.Core;

namespace KHRMS.Services
{
    public class LeaveTypeService(IUnitOfWork unitOfWork, ISendEmailService emailRepository) : ILeaveTypeService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public ISendEmailService _sendEmailService = emailRepository;



        public async Task<bool> AddLeaveType(LeaveType leaveType)

        {

            if (leaveType != null)

            {

                leaveType.CreatedDate = DateTime.Now;

                await _unitOfWork.LeaveType.Add(leaveType);

                var result = _unitOfWork.Save();

                if (result > 0)

                    return true;

                else

                    return false;

            }

            return false;

        }

        //public async Task<bool> AddLeaveType(LeaveType leaveType)
        //{
        //    if (leaveType != null)
        //    {
        //        leaveType.CreatedDate = DateTime.Now;
        //        await _unitOfWork.LeaveType.Add(leaveType);



        //            // Fetch Employee details using EmployeeId
        //            var employee = await _unitOfWork.Employees.GetById(leaveType.EmployeeId);

        //            if (employee == null)
        //            {
        //                throw new Exception("Employee not found.");
        //            }

        //            var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t=>t.Id == employee.ManagerId);

        //            if (manager == null)
        //            {
        //                throw new Exception("Manager not found.");
        //            }

        //            // Email parameters from database
        //            var startDate = new DateTime(2025, 4, 10);
        //            var endDate = new DateTime(2025, 4, 12);
        //            var managerEmail = manager.EmailAddress; 
        //            var managerName = $"{manager.FirstName} {manager.LastName}";
        //            var employeeName = $"{employee.FirstName} {employee.LastName}";
        //            var leaveReason = "LeaveRequest";

        //            // Create dictionary for placeholders
        //            var dict = new Dictionary<string, string>
        //    {
        //        { "ManagerName", managerName },
        //        { "StartDate", startDate.ToString("yyyy-MM-dd") },
        //        { "EndDate", endDate.ToString("yyyy-MM-dd") },
        //        { "LeaveReason", leaveReason },
        //        { "EmployeeName", employeeName },
        //        { "ManagerEmail", managerEmail }
        //    };

        //            // Email subject
        //            var subject = $"Attendance Request from {employeeName}";

        //            // Send email
        //            await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, leaveReason);

        //            return true;
        //        }
        //    var result = _unitOfWork.Save();

        //    return false;
        //}

        public async Task<bool> DeleteLeaveType(long LeaveTypeId)
        {
            if (LeaveTypeId > 0)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(LeaveTypeId);
                if (leaveTypeDetail != null)
                {
                    leaveTypeDetail.IsDeleted = true;
                    leaveTypeDetail.IsActive = false;

                    _unitOfWork.LeaveType.Update(leaveTypeDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                       
                }
            }
            return false;
        }

        public async Task<IEnumerable<LeaveType>> GetAllLeaveType()
        {
            var leaveTypeDetail = await _unitOfWork.LeaveType.GetAll();
            return leaveTypeDetail;
        }

        public async Task<LeaveType> GetLeaveTypeById(int LeaveTypeId)
        {
            if (LeaveTypeId > 0)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(LeaveTypeId);
                if (leaveTypeDetail != null)
                {
                    return leaveTypeDetail;
                }
            }
            return null;
        }

        public async Task<bool> UpdateLeaveType(LeaveType leaveType)
        {
            if (leaveType != null)
            {
                var leaveTypeDetail = await _unitOfWork.LeaveType.GetById(leaveType.Id);
                if (leaveTypeDetail != null)
                {
                   
                    leaveTypeDetail.Description = leaveType.Description;
                    leaveTypeDetail.Type = leaveType.Type;
                    leaveTypeDetail.UpdatedDate = DateTime.Now;
                    // ✅ Ensure IsActive status is updated
                    leaveTypeDetail.IsActive = leaveType.IsActive;
                    _unitOfWork.LeaveType.Update(leaveTypeDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
    }
}
