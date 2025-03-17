using KHRMS.Core;
using KHRMS.Services;
using Moq;
using NPOI.SS.Formula.Functions;
namespace KHRMS.UnitTest.ServiceTests
{
    public class EmployeePaymentInfoServiceTest
    {
        private readonly Mock<IEmployeePaymentInfoService> _mockService;
        private readonly IEmployeePaymentInfoService _service;

        public EmployeePaymentInfoServiceTest()
        {
            _mockService = new Mock<IEmployeePaymentInfoService>();
            _service = _mockService.Object;
        }

        [Fact]
        public async Task AddEmployeePayment_ReturnsSuccess()
        {
            var employeePayment = new EmployeePaymentInfo { Id = 1, EmployeeId = 1001 };
            _mockService.Setup(x => x.AddAsync(employeePayment)).Returns(Task.CompletedTask);

            await _service.AddAsync(employeePayment);
            _mockService.Verify(x => x.AddAsync(employeePayment), Times.Once);
        }

        [Fact]
        public async Task AddEmployeePayment_ThrowsException()
        {
            var employeePayment = new EmployeePaymentInfo { Id = 1, EmployeeId = 1001 };
            _mockService.Setup(x => x.AddAsync(employeePayment)).ThrowsAsync(new Exception("Unexpected error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.AddAsync(employeePayment));
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public async Task GetAllEmployeePayments_ReturnsSuccess()
        {
            var employeePayments = new List<EmployeePaymentInfo>
            {
                new EmployeePaymentInfo { Id = 1, EmployeeId = 1001 },
                new EmployeePaymentInfo { Id = 2, EmployeeId = 1002 }   
            };
            _mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(employeePayments);

            var result = await _service.GetAllAsync();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetEmployeePaymentById_ReturnsSuccess()
        {
            var employeePayment = new EmployeePaymentInfo { Id = 1, EmployeeId = 1001 };
            _mockService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employeePayment);

            var result = await _service.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetEmployeePaymentById_ReturnsNotFound()
        {
            _mockService.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((EmployeePaymentInfo)null);

            var result = await _service.GetByIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateEmployeePayment_ReturnsSuccess()
        {
            var employeePayment = new EmployeePaymentInfo { Id = 1, EmployeeId = 1001 };
            _mockService.Setup(x => x.UpdateAsync(employeePayment)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(employeePayment);
            _mockService.Verify(x => x.UpdateAsync(employeePayment), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployeePayment_ReturnsSuccess()
        {
            var Id = 1;
            _mockService.Setup(x => x.DeleteAsync(Id)).Returns(Task.CompletedTask);

            await _service.DeleteAsync(Id);
            _mockService.Verify(x => x.DeleteAsync(Id), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployeePayment_ReturnsNotFound()
        {
            var Id = 999;
            _mockService.Setup(x => x.DeleteAsync(Id)).ThrowsAsync(new KeyNotFoundException("Employee payment not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(Id));
            Assert.Equal("Employee payment not found", exception.Message);
        }
    }
}