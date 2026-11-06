using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class EmployeeHierarchyTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IEmployeeRepository> _empRepoMock;
        private readonly Mock<ISendEmailService> _emailMock;
        private readonly Mock<ITokenService> _tokenMock;
        private readonly Mock<IAuditService> _auditMock;
        private readonly EmployeeService _service;

        public EmployeeHierarchyTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _empRepoMock = new Mock<IEmployeeRepository>();
            _emailMock = new Mock<ISendEmailService>();
            _tokenMock = new Mock<ITokenService>();
            _auditMock = new Mock<IAuditService>();

            _uowMock.Setup(u => u.Employees).Returns(_empRepoMock.Object);
            _uowMock.Setup(u => u.EmployeeRoleMappings).Returns(new Mock<IEmployeeRoleMappingRepository>().Object);
            _uowMock.Setup(u => u.Save()).Returns(1);

            _service = new EmployeeService(
                _uowMock.Object,
                _emailMock.Object,
                _tokenMock.Object,
                _auditMock.Object);
        }

        [Fact]
        public async Task UpdateEmployee_WhenAssigningSelfAsManager_ThrowsInvalidOperationException()
        {
            var emp = new Employee { Id = 10, FirstName = "Alice", LastName = "Smith", IsDeleted = false, IsActive = true };
            _empRepoMock.Setup(r => r.GetById(10)).ReturnsAsync(emp);

            var req = new EmployeeRequestModel
            {
                Id = 10,
                ManagerId = 10 // Self assignment
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateEmployee(req));
            Assert.Contains("own reporting manager", ex.Message);
        }

        [Fact]
        public async Task UpdateEmployee_WhenDirectCircularHierarchy_ThrowsInvalidOperationException()
        {
            // Employee 1 reports to Employee 2
            var emp1 = new Employee { Id = 1, FirstName = "Alice", ManagerId = 2, IsDeleted = false, IsActive = true };
            // Employee 2 now attempts to report to Employee 1
            var emp2 = new Employee { Id = 2, FirstName = "Bob", ManagerId = null, IsDeleted = false, IsActive = true };

            var allEmployees = new List<Employee> { emp1, emp2 };

            _empRepoMock.Setup(r => r.GetById(2)).ReturnsAsync(emp2);
            _empRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(emp1);
            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(allEmployees.AsEnumerable());

            var req = new EmployeeRequestModel
            {
                Id = 2,
                ManagerId = 1 // 2 -> 1 while 1 -> 2
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateEmployee(req));
            Assert.Contains("Circular reporting hierarchy detected", ex.Message);
        }

        [Fact]
        public async Task UpdateEmployee_WhenIndirectCircularHierarchy_ThrowsInvalidOperationException()
        {
            // Chain: 1 -> 2 -> 3 -> 4
            var emp1 = new Employee { Id = 1, FirstName = "Alice", ManagerId = 2, IsDeleted = false, IsActive = true };
            var emp2 = new Employee { Id = 2, FirstName = "Bob", ManagerId = 3, IsDeleted = false, IsActive = true };
            var emp3 = new Employee { Id = 3, FirstName = "Charlie", ManagerId = 4, IsDeleted = false, IsActive = true };
            var emp4 = new Employee { Id = 4, FirstName = "Diana", ManagerId = null, IsDeleted = false, IsActive = true };

            var allEmployees = new List<Employee> { emp1, emp2, emp3, emp4 };

            // Now Diana (4) tries to set manager as Alice (1)
            _empRepoMock.Setup(r => r.GetById(4)).ReturnsAsync(emp4);
            _empRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(emp1);
            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(allEmployees.AsEnumerable());

            var req = new EmployeeRequestModel
            {
                Id = 4,
                ManagerId = 1 // Loop: 4 -> 1 -> 2 -> 3 -> 4
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateEmployee(req));
            Assert.Contains("Circular reporting hierarchy detected", ex.Message);
        }

        [Fact]
        public async Task UpdateEmployee_WhenValidManagerAssigned_Succeeds()
        {
            var emp1 = new Employee { Id = 1, FirstName = "Alice", ManagerId = null, IsDeleted = false, IsActive = true };
            var emp2 = new Employee { Id = 2, FirstName = "Bob", ManagerId = null, IsDeleted = false, IsActive = true };

            var allEmployees = new List<Employee> { emp1, emp2 };

            _empRepoMock.Setup(r => r.GetById(2)).ReturnsAsync(emp2);
            _empRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(emp1);
            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(allEmployees.AsEnumerable());

            var req = new EmployeeRequestModel
            {
                Id = 2,
                ManagerId = 1 // 2 -> 1 (Valid)
            };

            var result = await _service.UpdateEmployee(req);
            Assert.True(result);
            Assert.Equal(1, emp2.ManagerId);
        }
    }
}
