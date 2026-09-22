using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("Notifications")]
    public class Notification : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long EmployeeId { get; set; } // 0 = broadcast to all approvers/managers

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "System"; // Leave, Attendance, Document, Timesheet, Asset, Banking, Salary, Interview, System

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "info"; // request, approval, info, alert

        [StringLength(50)]
        public string Icon { get; set; } = "notifications";

        [StringLength(30)]
        public string IconBg { get; set; } = "#eff6ff";

        [StringLength(30)]
        public string IconColor { get; set; } = "#2563eb";

        [StringLength(200)]
        public string Route { get; set; } = "/index/home";

        [StringLength(200)]
        public string? QueryParams { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime? ReadDate { get; set; }
    }
}
