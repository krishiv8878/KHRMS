using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long ActorEmployeeId { get; set; }

        [StringLength(200)]
        public string ActorEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, Logout, RoleAssign, Export, ViewSensitive

        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(100)]
        public string EntityId { get; set; } = string.Empty;

        public string? BeforeValuesJson { get; set; }

        public string? AfterValuesJson { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(300)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? RequestId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
