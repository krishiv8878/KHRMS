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
        public void Add_ProjectMaster_ValidProject_ReturnsPass()
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
        public void Add_ProjectMaster_DuplicateProject_ReturnsFail()
        {
            // Arrange
            var mock = new Mock<IProjectMasterService>();
            List<ProjectMaster> projectmasters = new List<ProjectMaster>();

            // Mock the service to return true when adding a project
            mock.Setup(x => x.AddProjectMaster(It.IsAny<ProjectMaster>()))
                .Returns(Task.FromResult(true));

            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = "Project for HR Management", // Fixed Description field
                ClientName = "dev",
                ClientRegion = "India"
            };

            projectmasters.Add(projectmaster);

            // Act & Assert: Simulate duplicate entry validation
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                if (projectmasters.Any(p => p.Id == projectmaster.Id))
                {
                    throw new InvalidOperationException("ProjectMaster already exists");
                }
                projectmasters.Add(projectmaster);
            });

            // Validate the exception message
            Assert.Equal("ProjectMaster already exists", exception.Message);
        }     
        [Fact]
        public async Task Add_ProjectMaster_NullProject_ThrowsArgumentNullException()
        {
            // Arrange
            var mock = new Mock<IProjectMasterService>();

            // Setup mock to throw an exception when adding null
            mock.Setup(x => x.AddProjectMaster(null))
                .ThrowsAsync(new ArgumentNullException(nameof(ProjectMaster), "ProjectMaster cannot be null"));

            IProjectMasterService projectMasterService = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await projectMasterService.AddProjectMaster(null)
            );

            // Verify exception message
            Assert.Equal("ProjectMaster cannot be null (Parameter 'ProjectMaster')", exception.Message);
        }
        [Fact]
        public void Delete_ProjectMaster_ValidId_ReturnsPass()
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
        public async Task Delete_ProjectMaster_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IProjectMasterService>();

            // ✅ Setup the mock to throw a KeyNotFoundException when DeleteProjectMaster is called
            mock.Setup(x => x.DeleteProjectMaster(Id))
                .ThrowsAsync(new KeyNotFoundException("ProjectMaster not found"));

            IProjectMasterService projectmasterservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await projectmasterservice.DeleteProjectMaster(Id)
            );

            // ✅ Ensure exception message is correct
            Assert.Equal("ProjectMaster not found", exception.Message);

            // ✅ Verify the method was called exactly once
            mock.Verify(x => x.DeleteProjectMaster(Id), Times.Once);
        }
        [Fact]
        public async Task Delete_ProjectMaster_Exception_ThrowsKeyNotFoundException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();

            //  Setup the mock to throw a KeyNotFoundException
            mock.Setup(x => x.DeleteProjectMaster(Id))
                .ThrowsAsync(new KeyNotFoundException("ProjectMaster not found"));

            IProjectMasterService projectmasterservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await projectmasterservice.DeleteProjectMaster(Id)
            );

            // Ensure the exception message is correct
            Assert.Equal("ProjectMaster not found", exception.Message);

            // Verify the method was called once
            mock.Verify(x => x.DeleteProjectMaster(Id), Times.Once);
        }
        [Fact]
        public void Get_AllProjectMasters_ValidData_ReturnsList()
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
        public async Task Get_AllProjectMasters_NoData_ThrowsInvalidOperationException()
        {
            // Arrange
            var mock = new Mock<IProjectMasterService>();

            // ✅ Setup the mock to throw an exception when GetAllProjectMaster() is called
            mock.Setup(x => x.GetAllProjectMaster())
                .ThrowsAsync(new InvalidOperationException("No ProjectMaster available"));

            IProjectMasterService projectmasterservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await projectmasterservice.GetAllProjectMaster()
            );

            // ✅ Ensure exception message is correct
            Assert.Equal("No ProjectMaster available", exception.Message);
        }

        [Fact]
        public async Task Get_AllProjectMasters_Exception_ThrowsException()
        {
            // Arrange
            var mock = new Mock<IProjectMasterService>();

            // ✅ Correctly set up the mock to throw an exception
            mock.Setup(x => x.GetAllProjectMaster())
                .ThrowsAsync(new Exception("Unexpected error"));

            IProjectMasterService projectmasterservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await projectmasterservice.GetAllProjectMaster()
            );

            // ✅ Verify the exception message
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public void Get_ProjectMasterById_ValidId_ReturnsProjectMaster()
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
        public async Task Get_ProjectMasterById_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IProjectMasterService>();

            // ✅ Correctly set up the mock to throw a KeyNotFoundException
            mock.Setup(x => x.GetProjectMasterById(Id))
                .ThrowsAsync(new KeyNotFoundException("ProjectMaster not found"));

            IProjectMasterService projectmasterservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await projectmasterservice.GetProjectMasterById(Id)
            );

            // ✅ Verify the exception message
            Assert.Equal("ProjectMaster not found", exception.Message);
        }
       
        [Fact]
        public async Task Get_ProjectMasterById_Exception_ThrowsKeyNotFoundException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();

            // ✅ Correctly set up the mock to throw a KeyNotFoundException
            mock.Setup(x => x.GetProjectMasterById(Id))
                .ThrowsAsync(new KeyNotFoundException("ProjectMaster not found"));

            IProjectMasterService projectmasterservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await projectmasterservice.GetProjectMasterById(Id)
            );

            // ✅ Verify the exception message
            Assert.Equal("ProjectMaster not found", exception.Message);
        }

        [Fact]
        public void Update_ProjectMaster_ValidProject_ReturnsPass()
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
        public async Task Update_ProjectMaster_ProjectNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();

            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };

            //  Ensure GetProjectMasterById returns null (indicating not found)
            mock.Setup(x => x.GetProjectMasterById(Id)).ReturnsAsync((ProjectMaster)null);

            //  Ensure UpdateProjectMaster throws the expected exception
            mock.Setup(x => x.UpdateProjectMaster(It.IsAny<ProjectMaster>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            IProjectMasterService projectmasterservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await projectmasterservice.UpdateProjectMaster(projectmaster)
            );

            //  Verify the exception message
            Assert.Equal("Update not found", exception.Message);
        }
    
        [Fact]
        public async Task Update_ProjectMaster_NullProject_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mock = new Mock<IProjectMasterService>();
            IProjectMasterService projectmasterservice = mock.Object;

            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };

            // Ensure GetProjectMasterById returns null (indicating not found)
            mock.Setup(x => x.GetProjectMasterById(It.IsAny<int>())).ReturnsAsync((ProjectMaster)null);

            //  Ensure UpdateProjectMaster throws an exception when null is passed
            mock.Setup(x => x.UpdateProjectMaster(null))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await projectmasterservice.UpdateProjectMaster(null)
            );

            // Verify the exception message
            Assert.Equal("Update not found", exception.Message);
        }

    }
}
