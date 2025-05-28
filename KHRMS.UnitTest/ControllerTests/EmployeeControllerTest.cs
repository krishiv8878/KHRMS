using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Request;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ControllerTests
{
    public class EmployeeControllerTest
    {
        private readonly Mock<IEmployeeService> _mock;
        private readonly EmployeeController _controller;
        public EmployeeControllerTest()
        {
            _mock  = new Mock<IEmployeeService>();
            _controller = new EmployeeController(_mock.Object);
        }

        [Fact]
        public void GetEmployeesReturnPass()
        {
            var employee = new List<EmployeeRequestModel>
            {
                new EmployeeRequestModel{
                    Id = 1,
                    EmployeeCode = 0,
                    FirstName = "Raj",
                    LastName = "Prajapati",
                    EmailAddress = "raj@gmail.com",
                    MobileNumber = "1234567890",
                    DesignationId = 0,
                    DateOfJoining = new DateTime(2024, 10, 16, 9, 44, 16),
                    Gender = "Male",
                    CurrentAddress = "Patan",
                    PermanentAddress = "patan",
                    RoleIds =[1,2] ,
                    rolenames=["Tester"],
                    ShiftId="1",
                    ManagerId=1,
                    ManagerName="Pushpak",
                    PrimaryEmailAddress="tester@gmail.com",
                }
            };
            _mock.Setup(x => x.GetAllEmployees()).ReturnsAsync(employee);
            var result = _controller.GetEmployees();
            Assert.NotNull(result);
            _mock.Verify(x => x.GetAllEmployees(), Times.Once());
        }

        [Fact]
        public void AddEmployeeReturnPass()
        {
            var employee = new EmployeeRequestModel()
            {
                Id = 1,
                EmployeeCode = 0,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "raj@gmail.com",
                MobileNumber = "1234567890",
                DesignationId = 0,
                DateOfJoining = new DateTime(2024, 10, 16, 9, 44, 16),
                Gender = "Male",
                CurrentAddress = "Patan",
                PermanentAddress = "patan",
                RoleIds = [1, 2],
                rolenames = ["Tester"],
                ShiftId = "1",
                ManagerId = 1,
                ManagerName = "Pushpak",
                PrimaryEmailAddress = "tester@gmail.com",
            };
            _mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>())).ReturnsAsync(true);
            var result = _controller.AddEmployee(employee);
            Assert.NotNull(result);
            _mock.Verify(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()), Times.Once());
        }

        [Fact]
        public void UpdateEmployeeReturnPass()
        {
            var employeeRequestModel = new EmployeeRequestModel()
            {
                Id = 1,
                EmployeeCode = 0,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "raj@gmail.com",
                MobileNumber = "1234567890",
                DesignationId = 0,
                DateOfJoining = new DateTime(2024, 10, 16, 9, 44, 16),
                Gender = "Male",
                CurrentAddress = "Patan",
                PermanentAddress = "patan",
                RoleIds = [1, 2],
                rolenames = ["Tester"],
                ShiftId = "1",
                ManagerId = 1,
                ManagerName = "Pushpak",
                PrimaryEmailAddress = "tester@gmail.com",
            };
            var updateemployee = new EmployeeRequestModel()
            {
                Id = 1,
                EmployeeCode = 0,
                FirstName = "Rajesh",
                LastName = "Prajapati",
                EmailAddress = "raj@gmail.com",
                MobileNumber = "1234567890",
                DesignationId = 0,
                DateOfJoining = new DateTime(2024, 10, 16, 9, 44, 16),
                Gender = "Male",
                CurrentAddress = "Patan",
                PermanentAddress = "patan",
                RoleIds = [1, 2],
                rolenames = ["Tester"],
                ShiftId = "1",
                ManagerId = 1,
                ManagerName = "Pushpak",
                PrimaryEmailAddress = "tester@gmail.com",
            };
            _mock.Setup(x => x.UpdateEmployee(It.IsAny<EmployeeRequestModel>())).ReturnsAsync(true);
            var result = _controller.UpdateEmployee(updateemployee);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateEmployee(It.Is<EmployeeRequestModel>(r =>
                r.Id == updateemployee.Id &&
                r.FirstName == updateemployee.FirstName)), Times.Once());
        }

        [Fact]
        public void DeleteEmployeeReturnPass()
        {
            Employee employee = new Employee()
            {
                Id = 1,
                EmployeeCode = 0,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "raj@gmail.com",
                MobileNumber = "1234567890",
                DesignationId = 0,
                DateOfJoining = new DateTime(2024, 10, 16, 9, 44, 16),
                Gender = "Male",
                CurrentAddress = "Patan",
                PermanentAddress = "patan"
            };
            _mock.Setup(x => x.DeleteEmployee(employee.Id)).ReturnsAsync(true);

            var result = _controller.DeleteEmployee(employee.Id);
            Assert.NotNull(result);

            _mock.Verify(x => x.DeleteEmployee(employee.Id), Times.Once);
        }

    }
}
