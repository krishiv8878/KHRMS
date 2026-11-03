using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Core
{

    public class LeaveType : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

       
        [StringLength(100)]
        [Required(ErrorMessage = "LeaveType is required")]
        public string? Type { get; set; }

        [StringLength(250)]
        [Required(ErrorMessage = "Leave Description is required")]
        public string? Description { get; set; }

        public int AllowedDays { get; set; } = 0;

        // Phase 5: Complete Leave Policy Configuration Engine
        public bool IsPaid { get; set; } = true;

        // Sandwich Rule Configuration (Admin configurable toggle & allowance count)
        public bool SandwichRuleEnabled { get; set; } = false;
        public int MaxSandwichLeavesAllowed { get; set; } = 1;

        // Negative Balance Policy
        public bool IsNegativeBalanceAllowed { get; set; } = false;
        [Column(TypeName = "decimal(18,2)")]
        public decimal NegativeBalanceLimit { get; set; } = 0;

        // Accrual Configuration
        public bool AccrualEnabled { get; set; } = true;
        [StringLength(30)]
        public string AccrualFrequency { get; set; } = "Monthly"; // Monthly, Quarterly, Annual, None
        [Column(TypeName = "decimal(18,2)")]
        public decimal AccrualAmount { get; set; } = 1.5m;

        // Rollover & Carry Forward Configuration
        public bool CarryForwardEnabled { get; set; } = true;
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxCarryForwardDays { get; set; } = 15;
        [StringLength(30)]
        public string ExcessBalanceTreatment { get; set; } = "Lapse"; // Lapse, Encash, Unlimited

        // Encashment Configuration
        public bool EncashmentAllowed { get; set; } = false;
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinBalanceForEncashment { get; set; } = 10;
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxEncashmentDaysPerRequest { get; set; } = 10;
        [StringLength(100)]
        public string EncashmentSalaryFormula { get; set; } = "Basic/30";

        // Compensatory Off (Comp-Off) Configuration
        public bool IsCompOff { get; set; } = false;
        public int CompOffValidityDays { get; set; } = 90;

        // Operational Limits
        public int NoticeDaysRequired { get; set; } = 0;
        public int MaxConsecutiveDays { get; set; } = 15;
        [StringLength(50)]
        public string ApplicableGender { get; set; } = "All"; // All, Male, Female
        [StringLength(50)]
        public string ApplicableEmploymentType { get; set; } = "All"; // All, Permanent, Probation, Contract
    }
}
