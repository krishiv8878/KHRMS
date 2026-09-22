using KHRMS.Services.Request;
using Microsoft.AspNetCore.Http;

namespace KHRMS.Services.Interfaces
{
    public interface IAssetRequestService
    {
        Task<AssetRequestResponseModel?> CreateAssetRequest(CreateAssetRequestModel model);
        Task<bool> UpdateAssetRequestStatus(UpdateAssetRequestStatusModel model);
        Task<List<AssetRequestResponseModel>> GetAssetRequests(long? employeeId = null);
        Task<AssetRequestResponseModel?> GetAssetRequestById(long id);
        Task<List<string>> UploadAssetImages(List<IFormFile> files);
    }
}
