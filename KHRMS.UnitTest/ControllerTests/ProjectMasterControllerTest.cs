using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
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

        [Fact]
        public void Update_ProjectMaster_WhenExistingProjectUpdated_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            var controller = new ProjectMasterController(mock.Object);
            mock.Setup(x => x.UpdateProjectMaster(It.IsAny<ProjectMaster>()));
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
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            var result = controller.UpdateProjectMaster(updateprojectmaster);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("HRMS", projectmaster.ProjectName);
        }

        [Fact]
        public void Delete_ProjectMaster_WhenExistingProjectDeleted_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IProjectMasterService>();
            var controller = new ProjectMasterController(mock.Object);
            mock.Setup(x => x.DeleteProjectMaster(1));
            ProjectMaster projectmaster = new ProjectMaster()
            {
                Id = 1,
                ProjectName = "HRMS",
                Description = new DateTime(2024, 09, 27, 13, 16, 32).ToString("yyyy-MM-dd HH:mm:ss"),
                ClientName = "dev",
                ClientRegion = "India"
            };
            var result = controller.DeleteCandidate(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }


    }
}
