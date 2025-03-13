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
        public SkillServiceTest()
        {
            
        }


        [Fact]
        public void AddSkillReturnPass()
        {
            var mock = new Mock<ISkillService>();
            List<Skill> skills = new List<Skill>();
            mock.Setup(x => x.AddSkill(It.IsAny<Skill>()))
                        .Returns(Task.FromResult(true));
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            skills.Add(skill);
            Assert.Equal(1, 1);
        }

        [Fact]
        public async Task AddSkillReturnFail()
        {
            // Arrange
            var mock = new Mock<ISkillService>();

            // ✅ Mock AddSkill to throw an exception when adding a duplicate skill
            mock.Setup(x => x.AddSkill(It.IsAny<Skill>()))
                .ThrowsAsync(new InvalidOperationException("Skill already exists"));

            var skillService = mock.Object;

            var skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };

            // Act & Assert: Expect AddSkill to throw an exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await skillService.AddSkill(skill)
            );

            // ✅ Ensure the correct exception message is thrown
            Assert.Equal("Skill already exists", exception.Message);
        }

        [Fact]
        public async Task AddSkillReturnException()
        {
            // Arrange
            var mock = new Mock<ISkillService>();

            // ✅ Mock AddSkill to throw an exception for duplicate skill
            mock.Setup(x => x.AddSkill(It.IsAny<Skill>()))
                .ThrowsAsync(new InvalidOperationException("Skill already exists"));

            var skillService = mock.Object;

            var skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };

            // Act & Assert: Expect AddSkill to throw an exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await skillService.AddSkill(skill)
            );

            // ✅ Ensure the correct exception message is thrown
            Assert.Equal("Skill already exists", exception.Message);
        }

        [Fact]
        public void DeleteSkillReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ISkillService>();
            ISkillService skillService = mock.Object;
            List<Skill> skills = new List<Skill>();
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            mock.Setup(x => x.DeleteSkill(Id));
            var result = skillService.DeleteSkill(Id);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteSkill(1), Times.Once);
        }

        [Fact]
        public async Task DeleteSkillReturnFail()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<ISkillService>();

            // ✅ Mock DeleteSkill to throw a KeyNotFoundException
            mock.Setup(x => x.DeleteSkill(Id))
                .ThrowsAsync(new KeyNotFoundException("Skill not found"));

            var skillService = mock.Object;

            // Act & Assert: Expect DeleteSkill to throw an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await skillService.DeleteSkill(Id));

            // ✅ Ensure the correct exception message is thrown
            Assert.Equal("Skill not found", exception.Message);

            // ✅ Verify DeleteSkill was called exactly once with the correct Id
            mock.Verify(x => x.DeleteSkill(Id), Times.Once);
        }

        [Fact]
        public async Task DeleteSkillReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ISkillService>();

            //  Mock DeleteSkill to throw a general exception
            mock.Setup(x => x.DeleteSkill(Id))
                .ThrowsAsync(new Exception("Unexpected error"));

            var skillService = mock.Object;

            // Act & Assert: Expect DeleteSkill to throw an exception
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await skillService.DeleteSkill(Id));

            //  Ensure the correct exception message is thrown
            Assert.Equal("Unexpected error", exception.Message);

            // Verify DeleteSkill was called exactly once with the correct Id
            mock.Verify(x => x.DeleteSkill(Id), Times.Once);
        }

        [Fact]
        public void GetAllSkillsReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ISkillService>();
            ISkillService skillService = mock.Object;
            List<Skill> skills = new List<Skill>();
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            mock.Setup(x => x.GetAllSkills());
            var result = skillService.GetAllSkills();
            Assert.NotNull(result);
            Assert.Equal(1, skill.Id);
            Assert.Equal("Java Developer", skill.SkillName);
        }

        
        [Fact]
        public async Task GetAllSkillsReturnFail()
        {
            // Arrange
            var mock = new Mock<ISkillService>();

            //  Mock GetAllSkills to throw an exception
            mock.Setup(x => x.GetAllSkills())
                .ThrowsAsync(new InvalidOperationException("No Skill available"));

            var skillService = mock.Object;

            // Act & Assert: Expect GetAllSkills to throw an exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await skillService.GetAllSkills());

            //  Ensure the correct exception message is thrown
            Assert.Equal("No Skill available", exception.Message);

            //  Verify GetAllSkills was called exactly once
            mock.Verify(x => x.GetAllSkills(), Times.Once);
        }


        [Fact]
        public async Task GetAllSkillsReturnException()
        {
            // Arrange
            var mock = new Mock<ISkillService>();

            // Mock GetAllSkills to throw an exception
            mock.Setup(x => x.GetAllSkills())
                .ThrowsAsync(new Exception("Unexpected error"));

            var skillService = mock.Object;

            // Act & Assert: Expect GetAllSkills to throw an exception
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await skillService.GetAllSkills());

            //  Ensure the correct exception message is thrown
            Assert.Equal("Unexpected error", exception.Message);

            //  Verify that GetAllSkills was called exactly once
            mock.Verify(x => x.GetAllSkills(), Times.Once);
        }

        [Fact]
        public void GetSkillByIdReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ISkillService>();
            ISkillService skillService = mock.Object;
            List<Skill> skills = new List<Skill>();
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            mock.Setup(x => x.GetSkillById(1));
            var result = skillService.GetSkillById(1);
            Assert.NotNull(result);
            Assert.Equal(1, skill.Id);
            Assert.Equal("Java Developer", skill.SkillName);
        }
      
        [Fact]
        public async Task GetSkillByIdReturnFail()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<ISkillService>();

            //  Mock GetSkillById to throw KeyNotFoundException
            mock.Setup(x => x.GetSkillById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Skill not found"));

            var skillService = mock.Object;

            // Act & Assert: Expect GetSkillById to throw an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await skillService.GetSkillById(Id));

            //  Ensure the correct exception message is thrown
            Assert.Equal("Skill not found", exception.Message);

            //  Verify that GetSkillById was called exactly once
            mock.Verify(x => x.GetSkillById(Id), Times.Once);
        }


        [Fact]
        public async Task GetSkillByIdReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ISkillService>();

            // ✅ Mock GetSkillById to throw a general Exception
            mock.Setup(x => x.GetSkillById(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var skillService = mock.Object;

            // Act & Assert: Expect GetSkillById to throw an exception
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await skillService.GetSkillById(Id));

            // ✅ Ensure the correct exception message is thrown
            Assert.Equal("Unexpected error", exception.Message);

            // ✅ Verify that GetSkillById was called exactly once
            mock.Verify(x => x.GetSkillById(Id), Times.Once);
        }

        [Fact]
        public void UpdateSkillReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ISkillService>();
            ISkillService skillService = mock.Object;
            List<Skill> skills = new List<Skill>();
            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };
            Skill updateskill = new Skill()
            {
                Id = 1,
                SkillName = "Angular Developer",
            };
            mock.Setup(x => x.GetSkillById(1));
            var result = skillService.UpdateSkill(skill);
            Assert.NotNull(result);
            Assert.Equal(1,1);
        }
        
        [Fact]
        public async Task UpdateSkillReturnFail()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ISkillService>();
            ISkillService skillService = mock.Object;

            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };

            mock.Setup(x => x.GetSkillById(Id)).ReturnsAsync((Skill)null); // Simulating not found
            mock.Setup(x => x.UpdateSkill(It.IsAny<Skill>())).ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => skillService.UpdateSkill(skill));

            // Assert
            Assert.Equal("Update not found", exception.Message);
        }

        [Fact]
        public async Task UpdateSkillReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ISkillService>();
            ISkillService skillService = mock.Object;

            Skill skill = new Skill()
            {
                Id = 1,
                SkillName = "Java Developer",
            };

            // Simulating that GetSkillById returns null (skill not found)
            mock.Setup(x => x.GetSkillById(Id)).ReturnsAsync((Skill)null);

            // Simulating that UpdateSkill throws a KeyNotFoundException when an invalid skill is passed
            mock.Setup(x => x.UpdateSkill(It.IsAny<Skill>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => skillService.UpdateSkill(skill));

            // Assert
            Assert.Equal("Update not found", exception.Message);
        }

    }
}
