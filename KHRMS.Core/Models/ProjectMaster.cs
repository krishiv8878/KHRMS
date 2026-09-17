using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class ProjectMaster : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "ProjectName is required")]
        public string? ProjectName { get; set; }

  
        [Required(ErrorMessage = "Description is required")]
        public String? Description { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "ClientName is required")]
        public string? ClientName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "ClientRegion is required")]
        public string? ClientRegion {  get; set; }

        public int TeamSize { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
        public long? ProjectManagerId { get; set; }

        [NotMapped]
        public string? ManagerName { get; set; }

        [NotMapped]
        public long? ManagerId
        {
            get => ProjectManagerId;
            set
            {
                if (value.HasValue && (!ProjectManagerId.HasValue || ProjectManagerId.Value == 0))
                {
                    ProjectManagerId = value;
                }
            }
        }
    }
}
