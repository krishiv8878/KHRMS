using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Core
{

    [Table("DocumentMaster")]

    public class DocumentMaster : KHRMSBase
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Employee Id")]
        public long EmployeeId { get; set; }

        [Required]
        [StringLength(255)]
        public string DocumentName { get; set; }
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; }
        [Required]
        public string FilePath { get; set; }
        public DateTime? UploadedDate { get; set; } = DateTime.UtcNow;
        public long UploadedBy { get; set; }

       
    }
}