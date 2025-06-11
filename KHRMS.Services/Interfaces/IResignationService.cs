using KHRMS.Core;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public interface IResignationService
    {
        Task<bool> AddResignations(ResignationRequestModel resignation);
        Task<IEnumerable<ResignationRequestModel>> GetAllResignations();
        Task<Resignation> GetResignationById(int resignationId);
        Task<bool> UpdateResignation(Resignation resignation);
        Task<bool> DeleteResignation(long ResignationId);
        Task<bool> ApproveOrRejectResignation(Resignation resignation);
    }
}
