using KHRMS.Core;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ControllerTests
{
    public class DesignationControllerTest
    {
        private readonly Mock<IDesignationService> _mock;
        private readonly DesignationController _controller;
        public DesignationControllerTest()
        {
            _mock = new Mock<IDesignationService>();
            _controller = new DesignationController(_mock.Object);
        }

        [Fact]
        public void Get_Designations_WhenCalled_ReturnsDesignationList()
        {
            var designation = new List<Designation>
            {
                new Designation {
                    Id = 8,
                    DesignationName = "DotnetCore"
                }
            };
            _mock.Setup(x => x.GetAllDesignations()).ReturnsAsync(designation);
            var result = _controller.GetDesignations();
            Assert.NotNull(result);
            _mock.Verify(x => x.GetAllDesignations(), Times.Once());
        }

        [Fact]
        public void Add_Designation_WhenValidDesignationProvided_ReturnsSuccess()
        {
            Designation designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"

            };
            _mock.Setup(x => x.CreateDesignation(It.IsAny<Designation>())).ReturnsAsync(true);
            var result = _controller.AddDesignation(designation);
            Assert.NotNull(result);
            _mock.Verify(x => x.CreateDesignation(It.IsAny<Designation>()), Times.Once());
        }

        [Fact]
        public void Update_Designation_WhenValidDesignationProvided_UpdatesSuccessfully()
        {
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
            _mock.Setup(x => x.UpdateDesignation(It.IsAny<Designation>())).ReturnsAsync(true);
            var result = _controller.UpdateDesignation(updatedesignation);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateDesignation(It.Is<Designation>(r =>
                r.Id == updatedesignation.Id &&
                r.DesignationName == updatedesignation.DesignationName)), Times.Once());
        }

        [Fact]
        public void Delete_Designation_WhenDesignationExists_DeletesSuccessfully()
        {
            Designation designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"

            };
            _mock.Setup(x => x.DeleteDesignation(designation.Id)).ReturnsAsync(true);

            var result = _controller.DeleteDesignation(designation.Id);
            Assert.NotNull(result);

            _mock.Verify(x => x.DeleteDesignation(designation.Id), Times.Once);
        }

    }
}
