using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("PmsGoals")]
    public class PmsGoal : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public long EmployeeId { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = "Individual"; // Individual, Department, Company

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Period { get; set; } = "Q4 2026"; // e.g. Q1 2026, Q2 2026, Annual 2026

        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 100;

        public int ProgressPercentage { get; set; } = 0; // 0 to 100

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "On Track"; // Not Started, On Track, At Risk, Behind, Completed

        public DateTime? DueDate { get; set; }
    }

    [Table("PmsKeyResults")]
    public class PmsKeyResult : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long GoalId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetValue { get; set; } = 100;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentValue { get; set; } = 0;

        [StringLength(30)]
        public string MetricUnit { get; set; } = "%"; // %, INR, Tasks, Clients, Score

        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 100;
    }

    [Table("PmsGoalCheckIns")]
    public class PmsGoalCheckIn : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long GoalId { get; set; }

        public long? KeyResultId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewValue { get; set; }

        public int ConfidenceScore { get; set; } = 3; // 1 to 5

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CheckInDate { get; set; } = DateTime.UtcNow;

        public long EmployeeId { get; set; }
    }
}
