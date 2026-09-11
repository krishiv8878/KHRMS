using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class TimesheetEntryDTO
    {
        public long Id { get; set; }

        [Required]
        public long ProjectId { get; set; }

        public string? ProjectName { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string TaskDescription { get; set; } = string.Empty;

        [Range(0.1, 24.0)]
        public decimal Hours { get; set; }
    }
}
