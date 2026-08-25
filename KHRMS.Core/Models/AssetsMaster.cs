using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class AssetsMaster : KHRMSBase
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "AssetsMasterName is required")]
        public string? AssetsMasterName { get; set; }


        [Required(ErrorMessage = "AssetsMaster Description is required")]
        public String? Description { get; set; }

        [Required(ErrorMessage = "SerialNumber IS required ")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "DateOfPurchase")]
        public DateTime? DateOfPurchase { get; set; }

        [StringLength(50)]
        public string? AssetType { get; set; } = "Laptop";
        public long? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        [StringLength(100)]
        public string? AssignedTo { get; set; } = "Unassigned";
        [StringLength(100)]
        public string? Location { get; set; } = "Main Office";
        [StringLength(50)]
        public string? Status { get; set; } = "Available";


    }
}
