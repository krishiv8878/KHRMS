using KHRMS.Core;

namespace KHRMS.Services
{
    public class RoleMasterService(IUnitOfWork unitOfWork) : IRoleMasterService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> AddRoleMaster(RoleMaster roleMaster)
        {
            if (roleMaster != null)
            {
                roleMaster.CreatedDate = DateTime.Now;
                await _unitOfWork.RoleMaster.Add(roleMaster);
                var result = _unitOfWork.Save();
                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public async Task<bool> DeleteRoleMaster(long roleMasterId)
        {
            if (roleMasterId > 0)
            {
                var roleMasterDetails = await _unitOfWork.RoleMaster.GetById(roleMasterId);
                if (roleMasterDetails != null)
                {
                    roleMasterDetails.IsDeleted = true;
                    roleMasterDetails.IsActive = false;

                    _unitOfWork.RoleMaster.Update(roleMasterDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<RoleMaster>> GetAllRoleMaster()
        {
            var roleMaster = await _unitOfWork.RoleMaster.GetAll();
            return roleMaster;
        }

        public async Task<RoleMaster> GetRoleMasterById(long RoleMasterId)
        {
            if(RoleMasterId > 0)
            {
                var roleMatserDetails = await _unitOfWork.RoleMaster.GetById(RoleMasterId);
                if(roleMatserDetails != null)
                {
                    return roleMatserDetails;
                }

            }
            return null;
        }

        public async Task<bool> UpdateRoleMaster(RoleMaster roleMaster)
        {
            if (roleMaster != null)
            {
                var roleMasterDetails = await _unitOfWork.RoleMaster.GetById(roleMaster.Id);
                if (roleMasterDetails != null)
                {
                    roleMasterDetails.RoleName = roleMaster.RoleName;
                    roleMasterDetails.UpdatedDate = DateTime.Now;
                    // ✅ Ensure IsActive status is updated
                    roleMasterDetails.IsActive = roleMaster.IsActive;
                    _unitOfWork.RoleMaster.Update(roleMasterDetails);
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
