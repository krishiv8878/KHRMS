using KHRMS.Core;

namespace KHRMS.Services
{
    public class DesignationService(IUnitOfWork unitOfWork) : IDesignationService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<bool> CreateDesignation(Designation designation)
        {
            if (designation != null)
            {
                designation.CreatedDate = DateTime.Now;
                await _unitOfWork.Designations.Add(designation);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }


        public async Task<bool> DeleteDesignation(long designationId)
        {
            if (designationId > 0)
            {
                var designationDetails = await _unitOfWork.Designations.GetById(designationId);
                if (designationDetails != null)
                {
                    designationDetails.IsDeleted = true;
                    designationDetails.IsActive = false;

                    _unitOfWork.Designations.Update(designationDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<Designation>> GetAllDesignations()
        {
            var designations = await _unitOfWork.Designations.GetAll();
            return designations;
        }

        public async Task<Designation> GetDesignationById(int designationId)
        {
            if (designationId > 0)
            {
                var designationDetails = await _unitOfWork.Designations.GetById(designationId);
                if (designationDetails != null)
                {
                    return designationDetails;
                }
            }
            return null;
        }

        public async Task<bool> UpdateDesignation(Designation designation)
        {
            if (designation != null)
            {
                var designationDetails = await _unitOfWork.Designations.GetById(designation.Id);
                if (designationDetails != null)
                {
                    designationDetails.DesignationName = designation.DesignationName;
                    designationDetails.DepartmentCategory = designation.DepartmentCategory;
                    designationDetails.CareerLevel = designation.CareerLevel;
                    designationDetails.UpdatedDate = DateTime.Now;

                    // ✅ Ensure IsActive status is updated
                    designationDetails.IsActive = designation.IsActive;
                    _unitOfWork.Designations.Update(designationDetails);
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
