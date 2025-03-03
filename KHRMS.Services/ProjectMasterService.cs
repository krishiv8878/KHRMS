using KHRMS.Core;

namespace KHRMS.Services
{
    public class ProjectMasterService(IUnitOfWork unitOfWork) : IProjectMasterService
        {
            public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> AddProjectMaster(ProjectMaster projectMaster)
        {
            if (projectMaster != null)
            {
                projectMaster.CreatedDate = DateTime.Now;
                await _unitOfWork.ProjectMasters.Add(projectMaster);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public async Task<bool> DeleteProjectMaster(long ProjectMasterId)
        {
            if (ProjectMasterId > 0)
            {
                var projectDetails = await _unitOfWork.ProjectMasters.GetById(ProjectMasterId);
                if (projectDetails != null)
                {
                    projectDetails.IsDeleted = true;
                    projectDetails.IsActive = false;

                    _unitOfWork.ProjectMasters.Update(projectDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

 

        public async Task<IEnumerable<ProjectMaster>> GetAllProjectMaster()
        {
            var projectDetails = await _unitOfWork.ProjectMasters.GetAll();
            return projectDetails;
        }

        public async Task<ProjectMaster> GetProjectMasterById(int ProjectMasterId)
        {
            if (ProjectMasterId > 0)
            {
                var projectDetail = await _unitOfWork.ProjectMasters.GetById(ProjectMasterId);
                if (projectDetail != null)
                {
                    return projectDetail;
                }
            }
            return null;
        }

        public async Task<bool> UpdateProjectMaster(ProjectMaster projectMaster)
        {
            if (projectMaster != null)
            {
                var projectDetail = await _unitOfWork.ProjectMasters.GetById(projectMaster.Id);
                if (projectDetail != null)
                {
                    projectDetail.ProjectName = projectMaster.ProjectName;
                    projectDetail.Description = projectMaster.Description;
                    projectDetail.ClientName = projectMaster.ClientName;    
                    projectDetail.ClientRegion = projectMaster.ClientRegion;
                    projectDetail.UpdatedDate = DateTime.Now;
                    // ✅ Ensure IsActive status is updated
                    projectDetail.IsActive = projectMaster.IsActive;
                    _unitOfWork.ProjectMasters.Update(projectDetail);
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
