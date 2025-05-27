using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class SkillControllerTest
    {
        private readonly Mock<ISkillService> _mock;
        private readonly SkillController _controller;
        public SkillControllerTest()
        {
            _mock = new Mock<ISkillService>();
            _controller = new SkillController(_mock.Object);
        }

        [Fact]
        public void Get_AllSkills_ShouldReturnSuccess_WhenSkillsExist()
        {
            var skill = new List<Skill>
            {
                new Skill{
                    Id = 1,
                    SkillName = "Java Developer",
                }
            };
            _mock.Setup(x => x.GetAllSkills()).ReturnsAsync(skill);
            var result = _controller.GetSkill();
            Assert.NotNull(result);
            _mock.Verify(x => x.GetAllSkills(), Times.Once());
        }

        [Fact]
        public void Update_Skill_ShouldReturnSuccess_WhenSkillIsUpdated()
        {
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            Skill updateskill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            _mock.Setup(x => x.UpdateSkill(It.IsAny<Skill>())).ReturnsAsync(true);
            var result = _controller.UpdateSkill(updateskill);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateSkill(It.Is<Skill>(r =>
                r.Id == updateskill.Id &&
                r.SkillName == updateskill.SkillName)), Times.Once());
        }


        [Fact]
        public void Add_Skill_ShouldReturnSuccess_WhenSkillIsAdded()
        {
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            _mock.Setup(x => x.AddSkill(It.IsAny<Skill>())).ReturnsAsync(true);
            var result = _controller.AddSkill(skill);
            Assert.NotNull(result);
            _mock.Verify(x => x.AddSkill(It.IsAny<Skill>()), Times.Once());
        }

        [Fact]
        public void Delete_Skill_ShouldReturnSuccess_WhenSkillIsDeleted()
        {
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            _mock.Setup(x => x.DeleteSkill(skill.Id)).ReturnsAsync(true);

            var result = _controller.DeleteSkill(skill.Id);
            Assert.NotNull(result);

            _mock.Verify(x => x.DeleteSkill(skill.Id), Times.Once);
        }


    }
}