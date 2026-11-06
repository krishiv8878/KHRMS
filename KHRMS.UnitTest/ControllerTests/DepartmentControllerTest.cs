using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Controllers;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ControllerTests
{
    public class DepartmentControllerTest
    {
        private readonly Mock<IDepartmentService> _departmentServiceMock;
        private readonly Mock<IUserContextService> _userContextMock;
        private readonly DepartmentController _controller;

        public DepartmentControllerTest()
        {
            _departmentServiceMock = new Mock<IDepartmentService>();
            _userContextMock = new Mock<IUserContextService>();
            _userContextMock.Setup(u => u.GetCurrentEmployeeId()).Returns(100L);

            _controller = new DepartmentController(_departmentServiceMock.Object, _userContextMock.Object);
        }

        [Fact]
        public async Task GetDepartments_WhenCalled_ReturnsOkWithList()
        {
            var depts = new List<DepartmentDto>
            {
                new DepartmentDto { Id = 1, Name = "Engineering", Code = "ENG", EmployeeCount = 12 },
                new DepartmentDto { Id = 2, Name = "Human Resources", Code = "HR", EmployeeCount = 3 }
            };

            _departmentServiceMock.Setup(s => s.GetAllDepartmentsAsync()).ReturnsAsync(depts);

            var actionResult = await _controller.GetDepartments();
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<ApiResponse<IEnumerable<DepartmentDto>>>(okResult.Value);

            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            _departmentServiceMock.Verify(s => s.GetAllDepartmentsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDepartmentById_WhenExists_ReturnsOk()
        {
            var dept = new DepartmentDto { Id = 1, Name = "Engineering", Code = "ENG" };
            _departmentServiceMock.Setup(s => s.GetDepartmentByIdAsync(1)).ReturnsAsync(dept);

            var actionResult = await _controller.GetDepartmentById(1);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<ApiResponse<DepartmentDto>>(okResult.Value);

            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Engineering", response.Data?.Name);
        }

        [Fact]
        public async Task GetDepartmentById_WhenNotFound_ReturnsNotFound()
        {
            _departmentServiceMock.Setup(s => s.GetDepartmentByIdAsync(999)).ReturnsAsync((DepartmentDto?)null);

            var actionResult = await _controller.GetDepartmentById(999);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            var response = Assert.IsType<ApiResponse<DepartmentDto>>(notFoundResult.Value);

            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddDepartment_WhenValidRequest_ReturnsCreatedAtAction()
        {
            var req = new CreateDepartmentRequest
            {
                Name = "Finance & Accounts",
                Code = "FIN",
                CostCenter = "CC-FIN-01"
            };

            var created = new DepartmentDto
            {
                Id = 10,
                Name = req.Name,
                Code = req.Code,
                CostCenter = req.CostCenter
            };

            _departmentServiceMock.Setup(s => s.CreateDepartmentAsync(req, 100L)).ReturnsAsync(created);

            var actionResult = await _controller.AddDepartment(req);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
            var response = Assert.IsType<ApiResponse<DepartmentDto>>(createdResult.Value);

            Assert.Equal((int)HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(10, response.Data?.Id);
            _departmentServiceMock.Verify(s => s.CreateDepartmentAsync(req, 100L), Times.Once);
        }

        [Fact]
        public async Task UpdateDepartment_WhenValidRequest_ReturnsOk()
        {
            var req = new UpdateDepartmentRequest
            {
                Name = "Core Engineering",
                Code = "ENG",
                CostCenter = "CC-ENG-01",
                IsActive = true
            };

            var updated = new DepartmentDto
            {
                Id = 1,
                Name = req.Name,
                Code = req.Code,
                CostCenter = req.CostCenter
            };

            _departmentServiceMock.Setup(s => s.UpdateDepartmentAsync(1, req, 100L)).ReturnsAsync(updated);

            var actionResult = await _controller.UpdateDepartment(1, req);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<ApiResponse<DepartmentDto>>(okResult.Value);

            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Core Engineering", response.Data?.Name);
        }

        [Fact]
        public async Task DeleteDepartment_WhenAssignedEmployeesExist_ReturnsBadRequest()
        {
            _departmentServiceMock.Setup(s => s.DeleteDepartmentAsync(1, 100L))
                .ThrowsAsync(new System.InvalidOperationException("Cannot delete department with assigned employees."));

            var actionResult = await _controller.DeleteDepartment(1);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
            var response = Assert.IsType<ApiResponse<bool>>(badRequest.Value);

            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(response.Data);
        }

        [Fact]
        public async Task Backfill_WhenTriggered_ReturnsOkWithCount()
        {
            _departmentServiceMock.Setup(s => s.BackfillDepartmentsFromExistingEmployeesAsync()).ReturnsAsync(42);

            var actionResult = await _controller.Backfill();
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<ApiResponse<int>>(okResult.Value);

            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, response.Data);
        }
    }
}
