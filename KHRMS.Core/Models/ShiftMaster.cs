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
        public String? StartTime { get; set; } //change type to TimeSpan from String
        public String? EndTime { get; set; }// change type to TimeSpan from String

        //remove columns IsActive,IsDeleted,UpdatedBy,UpdatedDate
    }
}
