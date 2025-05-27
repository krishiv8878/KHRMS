using KHRMS.Core;
using KHRMS.Infrastructure.Migrations;
using KHRMS.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ServiceTests
{
    public class ProjectMasterServiceTest
    {
        private readonly Mock<IProjectMasterService> _mock;
        private readonly IProjectMasterService _service;
        public ProjectMasterServiceTest()
        {
               _mock = new Mock<IProjectMasterService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task Add_ProjectMaster_ValidProject_ShouldReturnSuccess()
        {
            var project = new ProjectMaster
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = "2024-09-27 13:16:32",
                ClientName = "dev",
                ClientRegion = "India"
            };

            _mock.Setup(x => x.AddProjectMaster(project)).ReturnsAsync(true);

            var result = await _service.AddProjectMaster(project);
            Assert.True(result);
            _mock.Verify(x => x.AddProjectMaster(project), Times.Once);
        }
        [Fact]
        public async Task Add_ProjectMaster_DuplicateProject_ShouldThrowException()
        {
            var project = new ProjectMaster { Id = 1, ProjectName = "HRMS", Description = "desc", ClientName = "dev", ClientRegion = "India" };

            _mock.Setup(x => x.AddProjectMaster(project))
                .ThrowsAsync(new InvalidOperationException("ProjectMaster already exists"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddProjectMaster(project));
            Assert.Equal("ProjectMaster already exists", exception.Message);
            _mock.Verify(x => x.AddProjectMaster(project), Times.Once);
        }

        [Fact]
        public async Task Add_ProjectMaster_NullProject_ShouldThrowArgumentNullException()
        {
            _mock.Setup(x => x.AddProjectMaster(null))
                .ThrowsAsync(new ArgumentNullException(nameof(ProjectMaster), "ProjectMaster cannot be null"));

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddProjectMaster(null));
            Assert.Equal("ProjectMaster cannot be null (Parameter 'ProjectMaster')", exception.Message);
            _mock.Verify(x => x.AddProjectMaster(null), Times.Once);
        }

        [Fact]
        public async Task Delete_ProjectMaster_ValidId_ShouldReturnSuccess()
        {
            var id = 1;
            _mock.Setup(x => x.DeleteProjectMaster(id)).ReturnsAsync(true);

            var result = await _service.DeleteProjectMaster(id);
            Assert.True(result);
            _mock.Verify(x => x.DeleteProjectMaster(id), Times.Once);
        }

        [Fact]
        public async Task Delete_ProjectMaster_InvalidId_ShouldThrowKeyNotFoundException()
        {
            var id = 999;
            _mock.Setup(x => x.DeleteProjectMaster(id))
                .ThrowsAsync(new KeyNotFoundException("ProjectMaster not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteProjectMaster(id));
            Assert.Equal("ProjectMaster not found", exception.Message);
            _mock.Verify(x => x.DeleteProjectMaster(id), Times.Once);
        }

        [Fact]
        public async Task Get_AllProjectMasters_ValidData_ShouldReturnList()
        {
            var projects = new List<ProjectMaster>
        {
            new ProjectMaster { Id = 1, ProjectName = "HRMS", Description = "desc", ClientName = "dev", ClientRegion = "India" }
        };

            _mock.Setup(x => x.GetAllProjectMaster()).ReturnsAsync(projects);

            var result = await _service.GetAllProjectMaster();
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task Get_AllProjectMasters_Empty_ShouldThrowInvalidOperationException()
        {
            _mock.Setup(x => x.GetAllProjectMaster())
                .ThrowsAsync(new InvalidOperationException("No ProjectMaster available"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllProjectMaster());
            Assert.Equal("No ProjectMaster available", exception.Message);
        }

        [Fact]
        public async Task Get_ProjectMasterById_ValidId_ShouldReturnProject()
        {
            var project = new ProjectMaster { Id = 1, ProjectName = "HRMS", Description = "desc", ClientName = "dev", ClientRegion = "India" };
            _mock.Setup(x => x.GetProjectMasterById(1)).ReturnsAsync(project);

            var result = await _service.GetProjectMasterById(1);
            Assert.NotNull(result);
            Assert.Equal("HRMS", result.ProjectName);
        }

        [Fact]
        public async Task Get_ProjectMasterById_InvalidId_ShouldThrowKeyNotFoundException()
        {
            _mock.Setup(x => x.GetProjectMasterById(999))
                .ThrowsAsync(new KeyNotFoundException("ProjectMaster not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetProjectMasterById(999));
            Assert.Equal("ProjectMaster not found", exception.Message);
        }

        [Fact]
        public async Task Update_ProjectMaster_Valid_ShouldReturnSuccess()
        {
            var updatedProject = new ProjectMaster { Id = 1, ProjectName = "Updated", Description = "desc", ClientName = "dev", ClientRegion = "India" };
            _mock.Setup(x => x.UpdateProjectMaster(updatedProject)).ReturnsAsync(true);

            var result = await _service.UpdateProjectMaster(updatedProject);
            Assert.True(result);
            _mock.Verify(x => x.UpdateProjectMaster(updatedProject), Times.Once);
        }

        [Fact]
        public async Task Update_ProjectMaster_NotFound_ShouldThrowKeyNotFoundException()
        {
            var project = new ProjectMaster { Id = 1, ProjectName = "HRMS" };

            _mock.Setup(x => x.UpdateProjectMaster(project))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateProjectMaster(project));
            Assert.Equal("Update not found", exception.Message);
            _mock.Verify(x => x.UpdateProjectMaster(project), Times.Once);
        }

        [Fact]
        public async Task Update_ProjectMaster_Null_ShouldThrowKeyNotFoundException()
        {
            _mock.Setup(x => x.UpdateProjectMaster(null))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateProjectMaster(null));
            Assert.Equal("Update not found", exception.Message);
            _mock.Verify(x => x.UpdateProjectMaster(null), Times.Once);
        }

    }
}
