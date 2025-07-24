using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController(ISkillService skillService) : ControllerBase
    {
        public readonly ISkillService _skillService = skillService;


        /// <summary>
        /// Get the List of Skills 
        /// </summary>
        [HttpGet("GetSkills")]
        public async Task<IActionResult> GetSkill()
        {
            Log.Information("SkillController - GetSkill called.");

            var skills = await _skillService.GetAllSkills();

            if (skills == null || !skills.Any())
            {
                Log.Warning("SkillController - No skills found.");
                return Ok(new ApiResponse<List<Skill>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.NoSkillFound,
                    Data = new List<Skill>()
                });
            }

            Log.Information("SkillController - {Count} skills retrieved.", skills.Count());

            return Ok(new ApiResponse<List<Skill>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.SkillFound,
                Data = skills.ToList()
            });
        }


        /// <summary>
        /// Add a new Skill
        /// </summary>
        [HttpPost("AddSkills")]
        public async Task<IActionResult> AddSkill([FromBody] Skill skill)
        {
            Log.Information("SkillController - AddSkill called.");

            var isSkillAdded = await _skillService.AddSkill(skill);

            if (isSkillAdded)
            {
                Log.Information("SkillController - Skill added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.SkillAdded,
                    Data = true
                });
            }

            Log.Warning("SkillController - Failed to add skill.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.SkillNotAdded,
                Data = false
            });
        }


        /// <summary>
        /// Update existing Skill
        /// </summary>
        [HttpPut("UpdateSkill")]
        public async Task<IActionResult> UpdateSkill([FromBody] Skill skill)
        {
            Log.Information("SkillController - UpdateSkill called for ID: {Id}", skill.Id);

            var isSkillEdited = await _skillService.UpdateSkill(skill);

            if (isSkillEdited)
            {
                Log.Information("SkillController - Skill updated successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.SkillUpdated,
                    Data = true
                });
            }

            Log.Warning("SkillController - Failed to update skill.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.SkillNotUpdated,
                Data = false
            });
        }


        /// <summary>
        /// Delete a Skill
        /// </summary>
        [HttpDelete("DeleteSkill")]
        public async Task<IActionResult> DeleteSkill(long skillId)
        {
            Log.Information("SkillController - DeleteSkill called for ID: {Id}", skillId);

            var isSkillDeleted = await _skillService.DeleteSkill(skillId);

            if (isSkillDeleted)
            {
                Log.Information("SkillController - Skill deleted successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.SkillDeleted,
                    Data = true
                });
            }

            Log.Warning("SkillController - Failed to delete skill with ID: {Id}", skillId);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.SkillNotDeleted,
                Data = false
            });
        }
    }
}
