using KHRMS.Core;

namespace KHRMS.Services
{

    public class SkillService(IUnitOfWork unitOfWork) : ISkillService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<bool> AddSkill(Skill skill)
        {
           if(skill != null)
            {
                skill.CreatedDate = DateTime.Now;
                await _unitOfWork.Skills.Add(skill);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public async Task<bool> DeleteSkill(long skillId)
        {
            if(skillId > 0)
            {
                var skillDetails = await _unitOfWork.Skills.GetById(skillId);
                if (skillDetails != null)
                {
                    skillDetails.IsDeleted = true;
                    skillDetails.IsActive = false;

                    _unitOfWork.Skills.Update(skillDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<Skill>> GetAllSkills()
        {
            var skills = await _unitOfWork.Skills.GetAll();
            return skills;
        }

        public async Task<Skill> GetSkillById(int skillId)
        {
            if (skillId > 0)
            {
                var skillDetails = await _unitOfWork.Skills.GetById(skillId);
                if (skillDetails != null)
                {
                    return skillDetails;
                }
            }
            return null;
        } 

       


        public async Task<bool> UpdateSkill(Skill skill)
        {
            if (skill != null)
            {
                var skillDetails = await _unitOfWork.Skills.GetById(skill.Id);
                if (skillDetails != null)
                {
                    skillDetails.SkillName = skill.SkillName;
                    skillDetails.UpdatedDate = DateTime.Now;

                    // Toggle IsActive state if it exists
                    skillDetails.IsActive = skill.IsActive;

                    _unitOfWork.Skills.Update(skillDetails);
                    var result =  _unitOfWork.Save(); // Ensure Save is awaited if it's async
                    return result > 0;
                }
            }
            return false;
        }

    }
}
