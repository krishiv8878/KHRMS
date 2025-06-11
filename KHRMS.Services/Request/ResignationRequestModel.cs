using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using KHRMS.Core;

namespace KHRMS.Services.Request
{
    public class ResignationRequestModel :  KHRMSBase
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }

        public string? Reason { get; set; }

        public bool Status { get; set; }

        public long ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public string? NoticePeriod { get; set; }

        public DateTime? Resignation_Date { get; set; }

        public List<long>? RoleId { get; set; }
        public List<string?> RoleName { get; set; }
    }
}
