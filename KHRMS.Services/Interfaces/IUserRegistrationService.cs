using KHRMS.Core;
using KHRMS.Services.Request;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public interface IUserRegistrationService
    {
        Task<bool> GetRegistrationByUser(UserRegistration userRegistration);
        Task<InviteEmployeeResponse> InviteEmployeeAsync(InviteEmployeeRequest request, long creatorEmployeeId);
        Task<VerifyInvitationResponse> VerifyInvitationTokenAsync(string token);
        Task<bool> AcceptInvitationAsync(AcceptInvitationRequest request);
    }
}
