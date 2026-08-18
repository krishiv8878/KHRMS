using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class ShiftMaster : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string ShiftName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Time is required")]
        public string? StartTime { get; set; } //changed type from TimeSpan to String
        [Required(ErrorMessage = "End Time is required")]
        public string? EndTime { get; set; }// changed type from TimeSpan to String

        //remove columns IsActive,IsDeleted,UpdatedBy,UpdatedDate
    }
}
