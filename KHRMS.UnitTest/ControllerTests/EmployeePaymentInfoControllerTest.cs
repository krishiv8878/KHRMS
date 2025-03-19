using KHRMS.Core;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NPOI.SS.Formula.Functions;
using System.Net;

namespace KHRMS.UnitTest.ControllerTests
{
    public class EmployeePaymentInfoControllerTest
    {
        private readonly Mock<IEmployeePaymentInfoService> _mockService;
        private readonly EmployeePaymentInfoController _controller;

        public EmployeePaymentInfoControllerTest()
        {
            _mockService = new Mock<IEmployeePaymentInfoService>();
            _controller = new EmployeePaymentInfoController(_mockService.Object);
        }

        [Fact]
        public async Task Get_All_ShouldReturnOkWithList()
        {
            // Arrange
            var paymentInfos = new List<EmployeePaymentInfo> { new EmployeePaymentInfo { Id = 1 } };
            _mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(paymentInfos);

            // Act
            var result = await _controller.GetAll();
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        [Fact]
        public async Task Get_ById_ShouldReturnOk_WhenEntityExists()
        {
            // Arrange
            var paymentInfo = new EmployeePaymentInfo { Id = 1 };
            _mockService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(paymentInfo);

            // Act
            var result = await _controller.GetById(1);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        [Fact]
        public async Task Get_ById_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync((EmployeePaymentInfo)null);

            // Act
            var result = await _controller.GetById(1);
            var notFoundResult = result as NotFoundObjectResult;

            // Assert
            Assert.NotNull(notFoundResult);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WhenValid()
        {
            // Arrange
            var newPaymentInfo = new EmployeePaymentInfo { Id = 1 };
            _mockService.Setup(service => service.AddAsync(newPaymentInfo)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(newPaymentInfo);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenValid()
        {
            // Arrange
            var updatedPaymentInfo = new EmployeePaymentInfo { Id = 1 };
            _mockService.Setup(service => service.UpdateAsync(updatedPaymentInfo)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(1, updatedPaymentInfo);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }


        [Fact]

        public async Task Delete_ShouldReturnOk_WhenEntityExists()
        {
            // Arrange
            var existingPaymentInfo = new EmployeePaymentInfo { Id = 1 };
            _mockService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(existingPaymentInfo);
            _mockService.Setup(service => service.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(1);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync((EmployeePaymentInfo)null);

            // Act
            var result = await _controller.Delete(1);
            var notFoundResult = result as NotFoundResult;

            // Assert
            Assert.NotNull(notFoundResult);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
        }
    }
}
