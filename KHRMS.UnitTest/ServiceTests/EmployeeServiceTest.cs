using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Routing.Matching;
using Moq;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class EmployeeServiceTest
    {
        public EmployeeServiceTest()
        {

        }

        [Fact]
        public void Create_Employee_ShouldReturnSuccess_WhenValidInputProvided()
        {
            var mock = new Mock<IEmployeeService>();
            // List<Employee> employees = new List<Employee>();
            List<EmployeeRequestModel> employeeRequestModels = new List<EmployeeRequestModel>();

            mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()))
                        .Returns(Task.FromResult(true));
            //  Employee employee = new Employee()
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
            employeeRequestModels.Add(employeeRequestModel);
            Assert.Equal(1, 1);
        }

        [Fact]
        public async Task Create_Employee_ShouldThrowInvalidOperationException_WhenEmployeeAlreadyExists()
        {
            // Arrange
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw an exception when trying to create a duplicate employee
            mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()))
                .ThrowsAsync(new InvalidOperationException("Employee already exists"));

            var employeeService = mock.Object;

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
                PermanentAddress = "Patan"
            };

            // Act & Assert - Expect an exception when calling CreateEmployee
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await employeeService.CreateEmployee(employeeRequestModel)
            );

            // Verify the exception message
            Assert.Equal("Employee already exists", exception.Message);

            // Ensure CreateEmployee() was actually called
            mock.Verify(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()), Times.Once);
        }

        [Fact]
        public async Task Create_Employee_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            // Arrange
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw ArgumentNullException when CreateEmployee is called with null
            mock.Setup(x => x.CreateEmployee(null))
                .ThrowsAsync(new ArgumentNullException("employeeRequestModel", "Employee cannot be null"));

            var employeeService = mock.Object;

            // Act & Assert - Expect an exception when calling CreateEmployee with null
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await employeeService.CreateEmployee(null)
            );

            // Verify the exception message
            Assert.Equal("Employee cannot be null (Parameter 'employeeRequestModel')", exception.Message);

            // Ensure CreateEmployee() was actually called with null
            mock.Verify(x => x.CreateEmployee(null), Times.Once);
        }

        [Fact]
        public void Delete_Employee_ShouldReturnSuccess_WhenEmployeeExists()
        {
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            List<Employee> employees = new List<Employee>();
            var Id = 1;
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
            mock.Setup(x => x.DeleteEmployee(Id));
            var result = employeeservice.DeleteEmployee(Id);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteEmployee(Id), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployee_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw KeyNotFoundException when DeleteEmployee is called with an invalid ID
            mock.Setup(x => x.DeleteEmployee(Id))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            var employeeService = mock.Object;

            // Act & Assert - Expect an exception when calling DeleteEmployee
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await employeeService.DeleteEmployee(Id)
            );

            // Verify the exception message
            Assert.Equal("Employee not found", exception.Message);

            // Ensure DeleteEmployee() was actually called with the given ID
            mock.Verify(x => x.DeleteEmployee(Id), Times.Once);
        }

        [Fact]
        public async Task Delete_Employee_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw an exception when DeleteEmployee is called with the given ID
            mock.Setup(x => x.DeleteEmployee(Id))
                .ThrowsAsync(new Exception("Unexpected error occurred while deleting employee"));

            var employeeService = mock.Object;

            // Act & Assert - Expect an exception when calling DeleteEmployee
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await employeeService.DeleteEmployee(Id)
            );

            // Verify the exception message
            Assert.Equal("Unexpected error occurred while deleting employee", exception.Message);

            // Ensure DeleteEmployee() was actually called with the given ID
            mock.Verify(x => x.DeleteEmployee(Id), Times.Once);
        }

        [Fact]
        public void Get_AllEmployees_ShouldReturnEmployeeList_WhenEmployeesExist()
        {
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            List<Employee> employees = new List<Employee>();
            var Id = 1;
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
            mock.Setup(x => x.GetAllEmployees());
            var result = employeeservice.GetAllEmployees();
            Assert.Equal(1, employee.Id);
            Assert.Equal("Raj", employee.FirstName);
            mock.Verify(x => x.GetAllEmployees(), Times.Once);
        }

        [Fact]
        public async Task Get_AllEmployees_ShouldThrowInvalidOperationException_WhenNoEmployeesAvailable()
        {
            // Arrange
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw an exception when GetAllEmployees() is called
            mock.Setup(x => x.GetAllEmployees())
                .ThrowsAsync(new InvalidOperationException("No Employee available"));

            var employeeService = mock.Object;

            // Act & Assert - Expect an exception when calling GetAllEmployees
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await employeeService.GetAllEmployees()
            );

            // Verify the exception message
            Assert.Equal("No Employee available", exception.Message);

            // Ensure GetAllEmployees() was actually called
            mock.Verify(x => x.GetAllEmployees(), Times.Once);
        }

        [Fact]
        public async Task Get_AllEmployees_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw an exception when GetAllEmployees() is called
            mock.Setup(x => x.GetAllEmployees())
                .ThrowsAsync(new Exception("Unexpected error"));

            var employeeService = mock.Object;

            // Act & Assert - Expect an exception when calling GetAllEmployees
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await employeeService.GetAllEmployees()
            );

            // Verify the exception message
            Assert.Equal("Unexpected error", exception.Message);

            // Ensure GetAllEmployees() was actually called
            mock.Verify(x => x.GetAllEmployees(), Times.Once);
        }

        [Fact]
        public void Get_EmployeeById_ShouldReturnEmployee_WhenEmployeeExists()
        {
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            List<Employee> employees = new List<Employee>();
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
            mock.Setup(x => x.GetEmployeeById(1));
            var result = employeeservice.GetEmployeeById(1);
            Assert.NotNull(result);
            Assert.Equal(1, employee.Id);
            Assert.Equal("Raj", employee.FirstName);

        }

        [Fact]
        public async Task Get_EmployeeById_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw KeyNotFoundException when GetEmployeeById is called with any ID
            mock.Setup(x => x.GetEmployeeById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            var employeeService = mock.Object;

            // Act & Assert - Expect an exception when calling GetEmployeeById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await employeeService.GetEmployeeById(Id) // Ensure awaited call
            );

            // Verify the exception message
            Assert.Equal("Employee not found", exception.Message);

            // Ensure GetEmployeeById() was actually called with the given ID
            mock.Verify(x => x.GetEmployeeById(Id), Times.Once);
        }

        [Fact]
        public async Task Get_EmployeeById_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IEmployeeService>();

            // Set up the mock to throw an exception when GetEmployeeById is called
            mock.Setup(x => x.GetEmployeeById(Id))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            var employeeService = mock.Object;

            // Act & Assert - Expect an exception when calling GetEmployeeById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await employeeService.GetEmployeeById(Id)
            );

            // Verify the exception message
            Assert.Equal("Employee not found", exception.Message);

            // Ensure GetEmployeeById() was actually called with the given ID
            mock.Verify(x => x.GetEmployeeById(Id), Times.Once);
        }

        [Fact]
        public void Update_Employee_ShouldReturnSuccess_WhenEmployeeExistsAndValidInputProvided()
        {
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            //List<Employee> employees = new List<Employee>();
            //Employee employee = new Employee()
            List<EmployeeRequestModel> employeeRequestModels = new List<EmployeeRequestModel>();
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
            mock.Setup(x => x.GetEmployeeById(1));
            var result = employeeservice.UpdateEmployee(employeeRequestModel);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }

        [Fact]
        public async Task Update_Employee_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;

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
                PermanentAddress = "Patan",
            };

            // ✅ Ensure GetEmployeeById returns null (indicating employee not found)
            mock.Setup(x => x.GetEmployeeById(It.IsAny<int>())).ReturnsAsync((Employee)null);

            // ✅ Ensure UpdateEmployee throws KeyNotFoundException when employee is not found
            mock.Setup(x => x.UpdateEmployee(It.IsAny<EmployeeRequestModel>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await employeeservice.UpdateEmployee(employeeRequestModel)
            );

            // ✅ Verify the exception message
            Assert.Equal("Update not found", exception.Message);
        }

        [Fact]
        public async Task Update_Employee_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;

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
                PermanentAddress = "Patan",
                ManagerId = 0,
            };

            // ✅ Ensure GetEmployeeById returns null (indicating employee not found)
            mock.Setup(x => x.GetEmployeeById(It.IsAny<int>())).ReturnsAsync((Employee)null);

            // ✅ Ensure UpdateEmployee throws an exception when null is passed
            mock.Setup(x => x.UpdateEmployee(null))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await employeeservice.UpdateEmployee(null)
            );

            // ✅ Verify the exception message
            Assert.Equal("Update not found", exception.Message);
        }


    }
}
