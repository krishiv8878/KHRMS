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
        public EmployeeControllerTest()
        {
                
        }

        [Fact]
        public void GetEmployeesReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IEmployeeService>();
            mock.Setup(x => x.GetAllEmployees());
            var controller = new EmployeeController(mock.Object);
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
            var result = controller.GetEmployees();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("Raj", employee.FirstName);
        }

        [Fact]
        public void AddEmployeeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IEmployeeService>();
            mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()));
            var controller = new EmployeeController(mock.Object);
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
            var result = controller.GetEmployees();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("Raj", employee.FirstName);
        }

        [Fact]
        public void UpdateEmployeeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IEmployeeService>();
            mock.Setup(x => x.UpdateEmployee(It.IsAny<EmployeeRequestModel>()));
            var controller = new EmployeeController(mock.Object);
          //  Employee employee = new Employee();
             //List<Employee> employees = new List<Employee>();
            EmployeeRequestModel employeeRequestModel = new EmployeeRequestModel()
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
            Employee updateemployee = new Employee()
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
                PermanentAddress = "patan"
            };
            var result = controller.UpdateEmployee(employeeRequestModel);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("Raj", employeeRequestModel.FirstName);
        }

        [Fact]
        public void DeleteEmployeeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IEmployeeService>();
            mock.Setup(x => x.DeleteEmployee(1));
            var controller = new EmployeeController(mock.Object);
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
            var result = controller.DeleteEmployee(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteEmployee(1), Times.Once);
        }

    }
}
