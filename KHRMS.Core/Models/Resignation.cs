using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class Resignation : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Employee")]
        [Required(ErrorMessage = "EmployeeId is required")]
        public long EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required(ErrorMessage ="Resignation Reason is required")]
        public string? Reason { get; set; }

        public bool Status { get; set; }

        public long ManagerId { get; set; }

        [Required(ErrorMessage ="NoticePeriod is required")]
        public string? NoticePeriod { get; set; }

        [Required(ErrorMessage ="ResignationDate is required")]
        public DateTime Resignation_Date { get; set; }

        [Required(ErrorMessage = "RoleId is MIssing")]
        public List<long>? RoleId { get; set; }

    }
}
