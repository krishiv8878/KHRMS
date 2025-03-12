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
        public ProjectMasterServiceTest()
        {
                
        }

        [Fact]
        public void AddProjectMasterReturnPass()
        {
            var mock = new Mock<IProjectMasterService>();
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            mock.Setup(x => x.AddProjectMaster(It.IsAny<ProjectMaster>()))
                        .Returns(Task.FromResult(true));
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion ="India"
            };
            projectmasters.Add(projectmaster);
            Assert.Equal(1, 1);
        }


        [Fact]
        public void AddProjectMasterReturnFail()
        {
            var mock = new Mock<IProjectMasterService>();
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            mock.Setup(x => x.AddProjectMaster(It.IsAny<ProjectMaster>()))
                        .Returns(Task.FromResult(true));
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            projectmasters.Add(projectmaster);
            var exception = Assert.Throws<InvalidOperationException>(() => projectmasters.Add(projectmaster));
            Assert.Equal("ProjectMaster already exists", exception.Message);
        }


        [Fact]
        public void AddProjectMasterReturnException()
        {
            var mock = new Mock<IProjectMasterService>();
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            mock.Setup(x => x.AddProjectMaster(It.IsAny<ProjectMaster>()))
                        .Returns(Task.FromResult(true));
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            projectmasters.Add(projectmaster);
            var exception = Assert.Throws<InvalidOperationException>(() => projectmasters.Add(null));
            Assert.Equal("ProjectMaster already exists", exception.Message);
        }


        [Fact]
        public void DeleteProjectMasterReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.DeleteProjectMaster(Id));
            var result = projectmasterservice.DeleteProjectMaster(Id);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteProjectMaster(1), Times.Once);
        }

        [Fact]
        public async Task DeleteProjectMasterReturnFail()
        {
            var Id = 999;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.DeleteProjectMaster(Id));
            var result = projectmasterservice.DeleteProjectMaster(Id);
            mock.Verify(x => x.DeleteProjectMaster(1), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => projectmasterservice.DeleteProjectMaster(Id));
            Assert.Equal("ProjectMaster not found", exception.Message);
        }

        [Fact]
        public async Task DeleteProjectMasterReturnException()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.DeleteProjectMaster(Id));
            var result = projectmasterservice.DeleteProjectMaster(Id);
            mock.Verify(x => x.DeleteProjectMaster(1), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => projectmasterservice.DeleteProjectMaster(Id));
            Assert.Equal("ProjectMaster not found", exception.Message);
        }

        [Fact]
        public void GetAllProjectMasterReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.GetAllProjectMaster());
            var result = projectmasterservice.GetAllProjectMaster();
            Assert.NotNull(result);
            Assert.Equal(1, projectmaster.Id);
            Assert.Equal("HRMS", projectmaster.ProjectName);
        }

        [Fact]
        public async Task GetAllProjectMasterReturnFail()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster();
            mock.Setup(x => x.GetAllProjectMaster());
            var result = projectmasterservice.GetAllProjectMaster();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => projectmasterservice.GetAllProjectMaster());
            Assert.Equal("No ProjectMaster available", exception.Message);
        }

        [Fact]
        public async Task GetAllProjectMasterReturnException()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster();
            mock.Setup(x => x.GetAllProjectMaster());
            var result = projectmasterservice.GetAllProjectMaster();
            var exception = await Assert.ThrowsAsync<Exception>(() => mock.Object.GetAllProjectMaster());
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public void GetProjectMasterByIdReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.GetProjectMasterById(1));
            var result = projectmasterservice.GetProjectMasterById(1);
            Assert.NotNull(result);
            Assert.Equal(1, projectmaster.Id);
            Assert.Equal("HRMS", projectmaster.ProjectName);
        }

        [Fact]
        public async Task GetProjectMasterByIdReturnFail()
        {
            var Id = 999;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            mock.Setup(x => x.GetProjectMasterById(1));
            var result = projectmasterservice.GetProjectMasterById(1);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => projectmasterservice.GetProjectMasterById(Id));
            Assert.Equal("LeaveType not found", exception.Message);
        }

        [Fact]
        public async Task GetProjectMasterByIdReturnException()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            mock.Setup(x => x.GetProjectMasterById(1));
            var result = projectmasterservice.GetProjectMasterById(1);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => projectmasterservice.GetProjectMasterById(Id));
            Assert.Equal("ProjectMaster not found", exception.Message);
        }

        [Fact]
        public void UpdateProjectMasterReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            ProjectMaster updateprojectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMSNew",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.GetProjectMasterById(1));
            var result = projectmasterservice.UpdateProjectMaster(projectmaster);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }

        [Fact]
        public async Task UpdateProjectMasterReturnFail()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.GetProjectMasterById(1));
            var result = projectmasterservice.UpdateProjectMaster(projectmaster);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => projectmasterservice.UpdateProjectMaster(projectmaster));
            Assert.Equal("Update not found", exception.Message);
        }

        [Fact]
        public async Task UpdateProjectMasterReturnException()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            mock.Setup(x => x.GetProjectMasterById(1));
            var result = projectmasterservice.UpdateProjectMaster(projectmaster);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => projectmasterservice.UpdateProjectMaster(null));
            Assert.Equal("Update not found", exception.Message);
        }
    }
}
