using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectMasterController(IProjectMasterService projectMasterService) : ControllerBase
    {
        public readonly IProjectMasterService _projectMasterService = projectMasterService;

        /// <summary>
        /// Get the list of projectMaster
        /// </summary>
        /// <returns></returns>   

        [HttpGet("GetProjectMaster")]
        public async Task<IActionResult> GetProjectMaster()
        {
            Log.Information("ProjectMasterController - GetProjectMaster called.");

            var projectList = await _projectMasterService.GetAllProjectMaster();

            if (projectList == null || !projectList.Any())
            {
                Log.Warning("ProjectMasterController - No project master records found.");
                return NotFound(new ApiResponse<List<ProjectMaster>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.ProjectMasterNotFound,
                    Data = null
                });
            }

            Log.Information("ProjectMasterController - {Count} project master records found.", projectList.Count());
            return Ok(new ApiResponse<List<ProjectMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.ProjectMasterFound,
                Data = projectList.ToList()
            });
        }


        /// <summary>
        /// Add a new ProjectMaster
        /// </summary>

        [HttpPost("AddProjectMaster")]
        public async Task<IActionResult> AddProjectMaster([FromBody] ProjectMaster projectMaster)
        {
            Log.Information("ProjectMasterController - AddProjectMaster called.");

            if (projectMaster == null)
            {
                Log.Warning("ProjectMasterController - Invalid project master object.");
                return BadRequest("Invalid data.");
            }

            var result = await _projectMasterService.AddProjectMaster(projectMaster);

            if (result)
            {
                Log.Information("ProjectMasterController - Project master added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ProjectMasterAdded,
                    Data = true
                });
            }

            Log.Warning("ProjectMasterController - Failed to add project master.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.ProjectMasterNotAdded,
                Data = false
            });
        }


        /// <summary>
        /// Update a existing candidate
        /// </summary>

        [HttpPut("UpdateProjectMaster/{id}")]
        public async Task<IActionResult> UpdateProjectMaster(long id, [FromBody] ProjectMaster projectMaster)
        {
            Log.Information("ProjectMasterController - UpdateProjectMaster called for ID: {Id}", id);

            if (id != projectMaster.Id)
            {
                Log.Warning("ProjectMasterController - ID mismatch: URL ID {Id}, Body ID {BodyId}", id, projectMaster.Id);
                return BadRequest("ID mismatch.");
            }

            var result = await _projectMasterService.UpdateProjectMaster(projectMaster);

            if (result)
            {
                Log.Information("ProjectMasterController - Project master updated for ID: {Id}", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ProjectMasterUpdated,
                    Data = true
                });
            }

            Log.Warning("ProjectMasterController - Failed to update project master for ID: {Id}", id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.ProjectMasterNotUpdated,
                Data = false
            });
        }

        /// <summary>
        /// Delete existing candidate
        /// </summary>

        [HttpDelete("DeleteProjectMaster/{id}")]
        public async Task<IActionResult> DeleteProjectMaster(long id)
        {
            Log.Information("ProjectMasterController - DeleteProjectMaster called for ID: {Id}", id);

            var result = await _projectMasterService.DeleteProjectMaster(id);

            if (result)
            {
                Log.Information("ProjectMasterController - Project master deleted for ID: {Id}", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.ProjectMasterDeleted,
                    Data = true
                });
            }

            Log.Warning("ProjectMasterController - Failed to delete project master for ID: {Id}", id);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.ProjectMasterNotDeleted,
                Data = false
            });
        }
    }
}