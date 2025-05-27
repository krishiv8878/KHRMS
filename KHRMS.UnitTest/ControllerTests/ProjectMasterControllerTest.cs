using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Org.BouncyCastle.Asn1.Cms;


namespace KHRMS.UnitTest.ControllerTests
{
    public class ProjectMasterControllerTest
    {
        private readonly Mock<IProjectMasterService> _mock;
        private readonly ProjectMasterController _controller;
        public ProjectMasterControllerTest()
        {
            _mock = new Mock<IProjectMasterService>();
            _controller = new ProjectMasterController(_mock.Object);
        }

        [Fact]
        public void Get_AllProjectMasters_WhenCalled_ShouldReturnSuccess()
        {
            var projectmaster = new List<ProjectMaster>
            {
                new ProjectMaster{
                    Id = 1,
                    ProjectName = "HRMS",
                   // Description = new DateTime(2024, 09, 27, 13, 16, 32),
                    Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                    ClientName = "dev",
                    ClientRegion = "India"
                }
            };
            _mock.Setup(x => x.GetAllProjectMaster()).ReturnsAsync(projectmaster);
            var result = _controller.GetProjectMaster();
            Assert.NotNull(result);
            _mock.Verify(x => x.GetAllProjectMaster(), Times.Once());
        }

        [Fact]
        public void Add_ProjectMaster_WhenValidInputProvided_ShouldReturnSuccess()
        {
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            _mock.Setup(x => x.AddProjectMaster(It.IsAny<ProjectMaster>())).ReturnsAsync(true);
            var result = _controller.AddProjectMaster(projectmaster);
            Assert.NotNull(result);
            _mock.Verify(x => x.AddProjectMaster(It.IsAny<ProjectMaster>()), Times.Once());
        }

        //[Fact]
        //public void Update_ProjectMaster_WhenExistingProjectUpdated_ShouldReturnSuccess()
        //{
        //    var Id = 1;
        //    var _mock = new Mock<IProjectMasterService>();
        //    var controller = new ProjectMasterController(_mock.Object);
        //    _mock.Setup(x => x.UpdateProjectMaster(It.IsAny<ProjectMaster>()));
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
            var updateprojectmaster = new ProjectMaster
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };

            // Fix: Return Task<bool>
            _mock.Setup(x => x.UpdateProjectMaster(updateprojectmaster)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateProjectMaster(updateprojectmaster);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Data);

            _mock.Verify(x=>x.UpdateProjectMaster(updateprojectmaster),Times.Once());
        }



        [Fact]
        public async Task Delete_ProjectMaster_WhenExistingProjectDeleted_ShouldReturnSuccess()
        {
            // Arrange
            var id = 1;
            // Setup to return true (successful deletion)
            _mock.Setup(x => x.DeleteProjectMaster(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProjectMaster(id);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Data);

            _mock.Verify(x=>x.DeleteProjectMaster(id), Times.Once());   
        }


    }
}
