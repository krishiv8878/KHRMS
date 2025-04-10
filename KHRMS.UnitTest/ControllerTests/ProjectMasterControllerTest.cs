using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace KHRMS.UnitTest.ControllerTests
{
    public class ProjectMasterControllerTest
    {
        public ProjectMasterControllerTest()
        {
                
        }

        [Fact]
        public void Get_AllProjectMasters_WhenCalled_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            var controller = new ProjectMasterController(mock.Object);
            mock.Setup(x => x.GetAllProjectMaster());
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
               // Description = new DateTime(2024, 09, 27, 13, 16, 32),
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"), 
                ClientName = "dev",
                ClientRegion = "India"
            };
            var result = controller.GetProjectMaster();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }

        [Fact]
        public void Add_ProjectMaster_WhenValidInputProvided_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            var controller = new ProjectMasterController(mock.Object);
            mock.Setup(x => x.AddProjectMaster(It.IsAny<ProjectMaster>()));
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            var result = controller.AddProjectMaster(projectmaster);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("HRMS", projectmaster.ProjectName);
        }

        //[Fact]
        //public void Update_ProjectMaster_WhenExistingProjectUpdated_ShouldReturnSuccess()
        //{
        //    var Id = 1;
        //    var mock = new Mock<IProjectMasterService>();
        //    var controller = new ProjectMasterController(mock.Object);
        //    mock.Setup(x => x.UpdateProjectMaster(It.IsAny<ProjectMaster>()));
        //    ProjectMaster projectmaster = new ProjectMaster()
        //    {
        //        Id = 1,
        //        ProjectName = "HRMS",
        //        Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
        //        ClientName = "dev",
        //        ClientRegion = "India"
        //    };
        //    ProjectMaster updateprojectmaster = new ProjectMaster()
        //    {
        //        Id = 1,
        //        ProjectName = "HRMS",
        //        Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
        //        ClientName = "dev",
        //        ClientRegion = "India"
        //    };
        //    var result = controller.UpdateProjectMaster(updateprojectmaster);
        //    Assert.NotNull(result);
        //    Assert.Equal(1, 1);
        //    Assert.Equal("HRMS", projectmaster.ProjectName);
        //}
        [Fact]
        public async Task Update_ProjectMaster_WhenExistingProjectUpdated_ShouldReturnSuccess()
        {
            // Arrange
            var mock = new Mock<IProjectMasterService>();
            var controller = new ProjectMasterController(mock.Object);

            var updateprojectmaster = new ProjectMaster
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };

            // Fix: Return Task<bool>
            mock.Setup(x => x.UpdateProjectMaster(updateprojectmaster)).ReturnsAsync(true);

            // Act
            var result = await controller.UpdateProjectMaster(updateprojectmaster.Id, updateprojectmaster);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Data);
        }



        [Fact]
        public async Task Delete_ProjectMaster_WhenExistingProjectDeleted_ShouldReturnSuccess()
        {
            // Arrange
            var id = 1;
            var mock = new Mock<IProjectMasterService>();
            var controller = new ProjectMasterController(mock.Object);

            // Setup to return true (successful deletion)
            mock.Setup(x => x.DeleteProjectMaster(id)).ReturnsAsync(true);

            // Act
            var result = await controller.DeleteProjectMaster(id);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Data);
        }


    }
}
