using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Core.Interfaces;
using KHRMS.Services;
using KHRMS.Services.Request;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class DepartmentServiceTest
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IDepartmentRepository> _deptRepoMock;
        private readonly Mock<IEmployeeRepository> _empRepoMock;
        private readonly DepartmentService _service;

        public DepartmentServiceTest()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _deptRepoMock = new Mock<IDepartmentRepository>();
            _empRepoMock = new Mock<IEmployeeRepository>();

            _uowMock.Setup(u => u.Departments).Returns(_deptRepoMock.Object);
            _uowMock.Setup(u => u.Employees).Returns(_empRepoMock.Object);
            _uowMock.Setup(u => u.Save()).Returns(1);

            _service = new DepartmentService(_uowMock.Object);
        }

        [Fact]
        public async Task CreateDepartmentAsync_WhenValid_AddsAndSaves()
        {
            var existingDepts = new List<Department>();
            _deptRepoMock.Setup(r => r.GetAll()).ReturnsAsync(existingDepts.AsEnumerable());

            Department captured = null!;
            _deptRepoMock.Setup(r => r.Add(It.IsAny<Department>()))
                .Callback<Department>(d =>
                {
                    d.Id = 5;
                    captured = d;
                })
                .Returns(Task.CompletedTask);

            _deptRepoMock.Setup(r => r.GetById(5))
                .ReturnsAsync(() => captured);

            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(Enumerable.Empty<Employee>());

            var request = new CreateDepartmentRequest
            {
                Name = "Quality Assurance",
                Code = "QA",
                CostCenter = "CC-QA-01",
                Description = "QA Team"
            };

            var result = await _service.CreateDepartmentAsync(request, 10);

            Assert.NotNull(result);
            Assert.Equal("Quality Assurance", result.Name);
            Assert.Equal("QA", result.Code);
            Assert.Equal(5, result.Id);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task CreateDepartmentAsync_WhenDuplicateName_ThrowsInvalidOperationException()
        {
            var existingDepts = new List<Department>
            {
                new Department { Id = 1, Name = "Quality Assurance", IsDeleted = false }
            };
            _deptRepoMock.Setup(r => r.GetAll()).ReturnsAsync(existingDepts.AsEnumerable());

            var request = new CreateDepartmentRequest
            {
                Name = "quality assurance",
                Code = "QA"
            };

            var ex = await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
                _service.CreateDepartmentAsync(request, 10));

            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task UpdateDepartmentAsync_WhenDuplicateName_ThrowsInvalidOperationException()
        {
            var deptToUpdate = new Department { Id = 1, Name = "Engineering", IsDeleted = false };
            var existingDepts = new List<Department>
            {
                deptToUpdate,
                new Department { Id = 2, Name = "Marketing", IsDeleted = false }
            };

            _deptRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(deptToUpdate);
            _deptRepoMock.Setup(r => r.GetAll()).ReturnsAsync(existingDepts.AsEnumerable());

            var request = new UpdateDepartmentRequest
            {
                Name = "Marketing"
            };

            var ex = await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
                _service.UpdateDepartmentAsync(1, request, 10));

            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_WhenEmployeesAssigned_ThrowsInvalidOperationException()
        {
            var dept = new Department { Id = 1, Name = "Engineering", IsDeleted = false };
            var employees = new List<Employee>
            {
                new Employee { Id = 101, DepartmentId = 1, IsDeleted = false }
            };

            _deptRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(dept);
            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(employees.AsEnumerable());

            var ex = await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
                _service.DeleteDepartmentAsync(1, 10));

            Assert.Contains("Cannot delete department", ex.Message);
            Assert.Contains("active employee(s) are assigned", ex.Message);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_WhenNoEmployeesAssigned_SoftDeletesSuccessfully()
        {
            var dept = new Department { Id = 1, Name = "Engineering", IsActive = true, IsDeleted = false };
            var employees = new List<Employee>();

            _deptRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(dept);
            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(employees.AsEnumerable());

            var result = await _service.DeleteDepartmentAsync(1, 10);

            Assert.True(result);
            Assert.True(dept.IsDeleted);
            Assert.False(dept.IsActive);
            _deptRepoMock.Verify(r => r.Update(dept), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task BackfillDepartmentsFromExistingEmployeesAsync_CreatesMissingDepartmentsAndAssignsIds()
        {
            var existingDepts = new List<Department>();
            var employees = new List<Employee>
            {
                new Employee { Id = 1, FirstName = "Alice", Branch = "Engineering", DepartmentId = null, IsDeleted = false },
                new Employee { Id = 2, FirstName = "Bob", Branch = "Engineering", DepartmentId = null, IsDeleted = false },
                new Employee { Id = 3, FirstName = "Charlie", Branch = "Human Resources", DepartmentId = null, IsDeleted = false }
            };

            _deptRepoMock.Setup(r => r.GetAll()).ReturnsAsync(existingDepts.AsEnumerable());
            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(employees.AsEnumerable());

            long nextId = 1;
            _deptRepoMock.Setup(r => r.Add(It.IsAny<Department>()))
                .Callback<Department>(d =>
                {
                    d.Id = nextId++;
                    existingDepts.Add(d);
                })
                .Returns(Task.CompletedTask);

            int updatedCount = await _service.BackfillDepartmentsFromExistingEmployeesAsync();

            Assert.Equal(3, updatedCount);
            Assert.True(existingDepts.Count >= 2);
            Assert.Contains(existingDepts, d => d.Name == "Engineering");
            Assert.Contains(existingDepts, d => d.Name == "Human Resources");
            Assert.True(employees.All(e => e.DepartmentId.HasValue));
            _uowMock.Verify(u => u.Save(), Times.AtLeastOnce);
        }
    }
}
