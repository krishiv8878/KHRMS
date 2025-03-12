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
        public void CreateEmployeeReturnPass()
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
        public void CreateEmployeeReturnFail()
        {
            var mock = new Mock<IEmployeeService>();
            List<EmployeeRequestModel> employeeRequestModels = new List<EmployeeRequestModel>();
            mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()))
                        .Returns(Task.FromResult(true));
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
            var exception = Assert.Throws<InvalidOperationException>(() => employeeRequestModels.Add(employeeRequestModel));
            Assert.Equal("Employee already exists", exception.Message);
        }

        [Fact]
        public void CreateEmployeeReturnException()
        {
            var mock = new Mock<IEmployeeService>();
            List<Employee> employees = new List<Employee>();
            mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()))
                        .Returns(Task.FromResult(true));
            Employee employee = new Employee();
            employees.Add(employee);
            var exception = Assert.Throws<ArgumentNullException>(() => employees.Add(null));
            Assert.Equal("Employee cannot be null", exception.Message);
        }

        [Fact]
        public void DeleteEmployeeReturnPass()
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
        public async Task DeleteEmployeeReturnFail()
        {
            var Id = 999;
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
            mock.Setup(x => x.DeleteEmployee(Id));
            var result = employeeservice.DeleteEmployee(Id);
            mock.Verify(x => x.DeleteEmployee(Id), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => employeeservice.DeleteEmployee(Id));
            Assert.Equal("Employee not found", exception.Message);
        }

        [Fact]
        public async Task DeleteEmployeeReturnException()
        {
            var Id = 1;
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
            mock.Setup(x => x.DeleteEmployee(Id));
            var result = employeeservice.DeleteEmployee(Id);
            mock.Verify(x => x.DeleteEmployee(Id), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => employeeservice.DeleteEmployee(Id));
            Assert.Equal("Employee not found", exception.Message);
        }


        [Fact]
        public void GetAllEmployeesReturnPass()
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
            Assert.Equal(1,employee.Id);
            Assert.Equal("Raj", employee.FirstName);
            mock.Verify(x => x.GetAllEmployees(), Times.Once);
        }


        [Fact]
        public async Task GetAllEmployeesReturnFail()
        {
            var Id = 1;
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            List<Employee> employees = new List<Employee>();
            Employee employee = new Employee();
            mock.Setup(x => x.GetAllEmployees());
            var result = employeeservice.GetAllEmployees();
            mock.Verify(x => x.GetAllEmployees(), Times.Once);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => employeeservice.GetAllEmployees());
            Assert.Equal("No Employee available", exception.Message);
        }

        [Fact]
        public async Task GetAllEmployeesReturnExceptiion()
        {
            var Id = 1;
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            List<Employee> employees = new List<Employee>();
            Employee employee = new Employee();
            mock.Setup(x => x.GetAllEmployees());
            var result = employeeservice.GetAllEmployees();
            mock.Verify(x => x.GetAllEmployees(), Times.Once);
            var exception = await Assert.ThrowsAsync<Exception>(() => mock.Object.GetAllEmployees());
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public void GetEmployeeByIdReturnPass()
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
        public async Task GetEmployeeByIdReturnFail()
        {
            var Id=999;
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            List<Employee> employees = new List<Employee>();
            Employee employee = new Employee();
            mock.Setup(x => x.GetEmployeeById(1));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => employeeservice.GetEmployeeById(Id));
            Assert.Equal("Employee not found", exception.Message);
        }

        [Fact]
        public async Task GetEmployeeByIdReturnException()
        {
            var Id = 1;
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            List<Employee> employees = new List<Employee>();
            Employee employee = new Employee();
            mock.Setup(x => x.GetEmployeeById(1));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => employeeservice.GetEmployeeById(Id));
            Assert.Equal("Employee not found", exception.Message);
        }

        [Fact]
        public void UpdateEmployeeReturnPass()
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
        public async Task UpdateEmployeeReturnFail()
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
                PermanentAddress = "patan",

            };
            mock.Setup(x => x.GetEmployeeById(1));
            var result = employeeservice.UpdateEmployee(employeeRequestModel);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => employeeservice.UpdateEmployee(employeeRequestModel));
            Assert.Equal("Update not found", exception.Message);
    }
        [Fact]
        public async Task UpdateEmployeeReturnException()
        {
            var mock = new Mock<IEmployeeService>();
            IEmployeeService employeeservice = mock.Object;
            // List<Employee> employees = new List<Employee>();
            // Employee employee = new Employee()

            List<EmployeeRequestModel> employeeRequestModels  = new List<EmployeeRequestModel>();
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
                PermanentAddress = "patan",
                ManagerId = 0,
            };
            mock.Setup(x => x.GetEmployeeById(1));
            var result = employeeservice.UpdateEmployee(employeeRequestModel);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => employeeservice.UpdateEmployee(null));
            Assert.Equal("Update not found", exception.Message);
        }

    }
}
