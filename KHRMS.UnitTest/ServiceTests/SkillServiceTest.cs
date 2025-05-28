using KHRMS.Core;
using KHRMS.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ServiceTests
{
    public class SkillServiceTest
    {
        private readonly Mock<ISkillService> _mock;
        private readonly ISkillService _service;
        public SkillServiceTest()
        {
            _mock = new Mock<ISkillService>();
            _service = _mock.Object;
        }


        [Fact]
        public async Task Add_Skill_ShouldReturnPass()
        {
            var skill = new Skill { Id = 1, SkillName = "Java Developer" };
            _mock.Setup(x => x.AddSkill(skill)).ReturnsAsync(true);

            var result = await _service.AddSkill(skill);
            Assert.True(result);
            _mock.Verify(x => x.AddSkill(skill), Times.Once);
        }

        [Fact]
        public async Task Add_Skill_ShouldReturnFail_WhenSkillAlreadyExists()
        {
            var skill = new Skill { Id = 1, SkillName = "Java Developer" };
            _mock.Setup(x => x.AddSkill(skill)).ThrowsAsync(new InvalidOperationException("Skill already exists"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddSkill(skill));
            Assert.Equal("Skill already exists", exception.Message);
        }

        [Fact]
        public async Task Add_Skill_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            var skill = new Skill { Id = 1, SkillName = "Java Developer" };
            _mock.Setup(x => x.AddSkill(skill)).ThrowsAsync(new InvalidOperationException("Skill already exists"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddSkill(skill));
            Assert.Equal("Skill already exists", exception.Message);
        }

        [Fact]
        public async Task Delete_Skill_ShouldReturnPass()
        {
            var Id = 1;
            _mock.Setup(x => x.DeleteSkill(Id)).ReturnsAsync(true);

            var result = await _service.DeleteSkill(Id);
            Assert.True(result);
            _mock.Verify(x => x.DeleteSkill(Id), Times.Once);
        }

        [Fact]
        public async Task Delete_Skill_ShouldReturnFail_WhenSkillNotFound()
        {
            var Id = 999;
            _mock.Setup(x => x.DeleteSkill(Id)).ThrowsAsync(new KeyNotFoundException("Skill not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteSkill(Id));
            Assert.Equal("Skill not found", exception.Message);
            _mock.Verify(x => x.DeleteSkill(Id), Times.Once);
        }

        [Fact]
        public async Task Delete_Skill_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            var Id = 1;
            _mock.Setup(x => x.DeleteSkill(Id)).ThrowsAsync(new Exception("Unexpected error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.DeleteSkill(Id));
            Assert.Equal("Unexpected error", exception.Message);
            _mock.Verify(x => x.DeleteSkill(Id), Times.Once);
        }

        [Fact]
        public async Task Get_AllSkills_ShouldReturnPass()
        {
            var skills = new List<Skill>
        {
            new Skill { Id = 1, SkillName = "Java Developer" }
        };
            _mock.Setup(x => x.GetAllSkills()).ReturnsAsync(skills);

            var result = await _service.GetAllSkills();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Java Developer", result.First().SkillName);
        }

        [Fact]
        public async Task Get_AllSkills_ShouldReturnFail_WhenNoSkillsAvailable()
        {
            _mock.Setup(x => x.GetAllSkills()).ThrowsAsync(new InvalidOperationException("No Skill available"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllSkills());
            Assert.Equal("No Skill available", exception.Message);
            _mock.Verify(x => x.GetAllSkills(), Times.Once);
        }

        [Fact]
        public async Task Get_AllSkills_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            _mock.Setup(x => x.GetAllSkills()).ThrowsAsync(new Exception("Unexpected error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.GetAllSkills());
            Assert.Equal("Unexpected error", exception.Message);
            _mock.Verify(x => x.GetAllSkills(), Times.Once);
        }

        [Fact]
        public async Task Get_SkillById_ShouldReturnPass()
        {
            var skill = new Skill { Id = 1, SkillName = "Java Developer" };
            _mock.Setup(x => x.GetSkillById(1)).ReturnsAsync(skill);

            var result = await _service.GetSkillById(1);
            Assert.NotNull(result);
            Assert.Equal("Java Developer", result.SkillName);
        }

        [Fact]
        public async Task Get_SkillById_ShouldReturnFail_WhenSkillNotFound()
        {
            _mock.Setup(x => x.GetSkillById(999)).ThrowsAsync(new KeyNotFoundException("Skill not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetSkillById(999));
            Assert.Equal("Skill not found", exception.Message);
            _mock.Verify(x => x.GetSkillById(999), Times.Once);
        }

        [Fact]
        public async Task Get_SkillById_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            _mock.Setup(x => x.GetSkillById(1)).ThrowsAsync(new Exception("Unexpected error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.GetSkillById(1));
            Assert.Equal("Unexpected error", exception.Message);
            _mock.Verify(x => x.GetSkillById(1), Times.Once);
        }

        [Fact]
        public async Task Update_Skill_ShouldReturnPass()
        {
            var skill = new Skill { Id = 1, SkillName = "Java Developer" };
            _mock.Setup(x => x.UpdateSkill(skill)).ReturnsAsync(true);

            var result = await _service.UpdateSkill(skill);
            Assert.True(result);
            _mock.Verify(x => x.UpdateSkill(skill), Times.Once);
        }

        [Fact]
        public async Task Update_Skill_ShouldReturnFail_WhenSkillNotFound()
        {
            var skill = new Skill { Id = 1, SkillName = "Java Developer" };
            _mock.Setup(x => x.GetSkillById(1)).ReturnsAsync((Skill)null);
            _mock.Setup(x => x.UpdateSkill(skill)).ThrowsAsync(new KeyNotFoundException("Update not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateSkill(skill));
            Assert.Equal("Update not found", exception.Message);
        }

        [Fact]
        public async Task Update_Skill_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            var skill = new Skill { Id = 1, SkillName = "Java Developer" };
            _mock.Setup(x => x.GetSkillById(1)).ReturnsAsync((Skill)null);
            _mock.Setup(x => x.UpdateSkill(skill)).ThrowsAsync(new KeyNotFoundException("Update not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateSkill(skill));
            Assert.Equal("Update not found", exception.Message);
        }

    }
}
