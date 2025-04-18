using KHRMS.Core;
using KHRMS.Infrastructure.Migrations;

namespace KHRMS.Services
{
    public class AssetsMasterService(IUnitOfWork unitOfWork) : IAssetsMasterService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> AddAssetsMaster(AssetsMaster assetsMaster)
        {
            if (assetsMaster != null)
            {
                assetsMaster.CreatedDate = DateTime.Now;
                await _unitOfWork.AssetsMasters.Add(assetsMaster);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public async Task<bool> DeleteAssetsMaster(long assetsMasterId)
        {
            if (assetsMasterId > 0)
            {
                var assetsMasterDetail = await _unitOfWork.AssetsMasters.GetById(assetsMasterId);
                if (assetsMasterDetail != null)
                {
                    assetsMasterDetail.IsDeleted = true;
                    assetsMasterDetail.IsActive = false;

                    _unitOfWork.AssetsMasters.Update(assetsMasterDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<AssetsMaster>> GetAllAssetsMaster()
        {
            var assetsMaster = await _unitOfWork.AssetsMasters.GetAll();
            return assetsMaster ;
        }

        public async Task<AssetsMaster> GetAssetsMasterById(int assetsMasterId)
        {
            if (assetsMasterId > 0)
            {
                var assetsMasterDetail = await _unitOfWork.AssetsMasters.GetById(assetsMasterId);
                if (assetsMasterDetail != null)
                {
                    return assetsMasterDetail;
                }
            }
            return null;
        }

        public async Task<bool> UpdateAssetsMaster(AssetsMaster assetsMaster)
        {
            if (assetsMaster != null)
            {
                var assetsMasterDetail = await _unitOfWork.AssetsMasters.GetById(assetsMaster.Id);
                if (assetsMasterDetail != null)
                {
                    assetsMasterDetail.AssetsMasterName = assetsMaster.AssetsMasterName;
                    assetsMasterDetail.Description = assetsMaster.Description;
                    assetsMasterDetail.UpdatedDate = DateTime.Now;
                    assetsMasterDetail.SerialNumber = assetsMaster.SerialNumber;
                    // ✅ Ensure IsActive status is updated
                    assetsMasterDetail.IsActive = assetsMaster.IsActive;
                    _unitOfWork.AssetsMasters.Update(assetsMasterDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

    }
}

