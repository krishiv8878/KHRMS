using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace KHRMS.Core
{
    [Table("EmployeePaymentInfo")]

    public class EmployeePaymentInfo : KHRMSBase
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("EmployeeId")]
        [Required(ErrorMessage = "Employee is required")]

        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "BankName is required")]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required(ErrorMessage = "IFSC Code is required")]
        [StringLength(20)]
        public string IFSCCode { get; set; }

        [Required(ErrorMessage = "Account Number is required")]
        public long AccountNumber { get; set; }

        public string NameOnAccount { get; set; }

    }
}

