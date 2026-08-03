using Microsoft.AspNetCore.Http;

namespace KHRMS.Services.Request
{
    public class ProfileImageRequest
    {
        public long EmployeeId { get; set; }

        public IFormFile? File { get; set; }
    }
}