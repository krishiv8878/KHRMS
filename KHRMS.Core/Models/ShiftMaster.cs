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
        public TimeOnly? StartTime { get; set; } //change type to TimeOnly from String
        public TimeOnly? EndTime { get; set; }// change type to TimeOnly from String

        //remove columns IsActive,IsDeleted,UpdatedBy,UpdatedDate
    }
}
