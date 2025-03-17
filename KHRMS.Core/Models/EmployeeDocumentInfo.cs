using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Core
{
    [Table("EmployeeDocuments")]

    public class EmployeeDocumentInfo: KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        [Required(ErrorMessage = "File Path is required")]
        public string? FilePath { get; set; }       
        public DateTime? UploadedDate { get; set; }
        public long UploadedBy { get; set; }

    }
}
