using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.Core.Models
{
    public class ApproveLeaveRequest
    {
        public long Id { get; set; }  // Primary Key      
       
        public bool IsApproved { get; set; }
    }
}
