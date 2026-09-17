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
                if (projectMaster.ProjectManagerId == null && projectMaster.ManagerId != null)
                {
                    projectMaster.ProjectManagerId = projectMaster.ManagerId;
                }
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
            var projectDetails = (await _unitOfWork.ProjectMasters.GetAll()).ToList();
            var managerIds = projectDetails
                .Where(p => p.ProjectManagerId.HasValue && p.ProjectManagerId > 0)
                .Select(p => (int)p.ProjectManagerId.Value)
                .Distinct()
                .ToList();

            if (managerIds.Any())
            {
                var allEmployees = await _unitOfWork.Employees.GetAll();
                var managersMap = allEmployees
                    .Where(e => managerIds.Contains((int)e.Id))
                    .ToDictionary(
                        e => (long)e.Id,
                        e => $"{e.FirstName ?? ""} {e.LastName ?? ""}".Trim()
                    );

                foreach (var p in projectDetails)
                {
                    if (p.ProjectManagerId.HasValue && managersMap.TryGetValue(p.ProjectManagerId.Value, out var name))
                    {
                        p.ManagerName = name;
                    }
                }
            }

            return projectDetails;
        }

        public async Task<ProjectMaster> GetProjectMasterById(int ProjectMasterId)
        {
            if (ProjectMasterId > 0)
            {
                var projectDetail = await _unitOfWork.ProjectMasters.GetById(ProjectMasterId);
                if (projectDetail != null)
                {
                    if (projectDetail.ProjectManagerId.HasValue && projectDetail.ProjectManagerId > 0)
                    {
                        var emp = await _unitOfWork.Employees.GetById((int)projectDetail.ProjectManagerId.Value);
                        if (emp != null)
                        {
                            projectDetail.ManagerName = $"{emp.FirstName ?? ""} {emp.LastName ?? ""}".Trim();
                        }
                    }
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
                    projectDetail.ProjectManagerId = projectMaster.ProjectManagerId ?? projectMaster.ManagerId;
                    projectDetail.TeamSize = projectMaster.TeamSize;
                    projectDetail.StartDate = projectMaster.StartDate;
                    projectDetail.EndDate = projectMaster.EndDate;
                    projectDetail.Status = projectMaster.Status;
                    projectDetail.UpdatedDate = DateTime.UtcNow;
                    // ✅ Ensure IsActive status is updated
                    projectDetail.IsActive = projectMaster.IsActive;
                    _unitOfWork.ProjectMasters.Update(projectDetail);
                    var result = _unitOfWork.Save();
                    if (result >= 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
    }
}
