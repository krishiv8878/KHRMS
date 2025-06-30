using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.Services.Interfaces
{
    public interface ITokenService
    {
        string GeneratePasswordResetToken(string email);
        public ClaimsPrincipal? ValidatePasswordResetToken(string token);
    }
}
