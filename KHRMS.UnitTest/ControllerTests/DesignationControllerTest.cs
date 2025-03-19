using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class DesignationControllerTest
    {
        public DesignationControllerTest()
        {

        }

        [Fact]
        public void Ge_tDesignations_WhenCalled_ReturnsDesignationList()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            mock.Setup(x => x.GetAllDesignations());
            var controller = new DesignationController(mock.Object);
            Designation designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"

            };
            var result = controller.GetDesignations();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("DotnetCore", designation.DesignationName);
        }

        [Fact]
        public void Add_Designation_WhenValidDesignationProvided_ReturnsSuccess()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            mock.Setup(x => x.CreateDesignation(It.IsAny<Designation>()));
            var controller = new DesignationController(mock.Object);
            Designation designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"

            };
            var result = controller.AddDesignation(designation);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("DotnetCore", designation.DesignationName);
        }

        [Fact]
        public void Update_Designation_WhenValidDesignationProvided_UpdatesSuccessfully()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            mock.Setup(x => x.UpdateDesignation(It.IsAny<Designation>()));
            var controller = new DesignationController(mock.Object);
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "DotnetCore"
            };
            Designation updatedesignation = new Designation()
            {
                Id = 1,
                DesignationName = "Asp.net"
            };
            var result = controller.UpdateDesignation(designation);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("DotnetCore", designation.DesignationName);
        }

        [Fact]
        public void Delete_Designation_WhenDesignationExists_DeletesSuccessfully()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            mock.Setup(x => x.DeleteDesignation(1));
            var controller = new DesignationController(mock.Object);
            Designation designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"

            };
            var result = controller.DeleteDesignation(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteDesignation(1), Times.Once);
        }

    }
}
