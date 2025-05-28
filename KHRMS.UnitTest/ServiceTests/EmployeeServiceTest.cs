using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Request;
using Moq;
using NPOI.SS.Formula.Functions;


namespace KHRMS.UnitTest.ServiceTests
{
    public class EmployeeServiceTest
    {
        private readonly Mock<IEmployeeService> _mock;
        private readonly IEmployeeService _service;
        public EmployeeServiceTest()
        {
            _mock = new Mock<IEmployeeService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task Create_Employee_ShouldReturnSuccess_WhenValidInputProvided()
        {
            _mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()))
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
            _mock.Setup(x => x.CreateEmployee(employeeRequestModel)).ReturnsAsync(true);

            var result = await _service.CreateEmployee(employeeRequestModel);
            Assert.True(result);
            _mock.Verify(x => x.CreateEmployee(employeeRequestModel), Times.Once);
        }

        [Fact]
        public async Task Create_Employee_ShouldThrowInvalidOperationException_WhenEmployeeAlreadyExists()
        {
            // Set up the _mock to throw an exception when trying to create a duplicate employee
            _mock.Setup(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()))
                .ThrowsAsync(new InvalidOperationException("Employee already exists"));

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
                async () => await _service.CreateEmployee(employeeRequestModel)
            );

            // Verify the exception message
            Assert.Equal("Employee already exists", exception.Message);

            // Ensure CreateEmployee() was actually called
            _mock.Verify(x => x.CreateEmployee(It.IsAny<EmployeeRequestModel>()), Times.Once);
        }

        [Fact]
        public async Task Create_Employee_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            // Set up the _mock to throw ArgumentNullException when CreateEmployee is called with null
            _mock.Setup(x => x.CreateEmployee(null))
                .ThrowsAsync(new ArgumentNullException("employeeRequestModel", "Employee cannot be null"));

            // Act & Assert - Expect an exception when calling CreateEmployee with null
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.CreateEmployee(null)
            );

            // Verify the exception message
            Assert.Equal("Employee cannot be null (Parameter 'employeeRequestModel')", exception.Message);

            // Ensure CreateEmployee() was actually called with null
            _mock.Verify(x => x.CreateEmployee(null), Times.Once);
        }

        [Fact]
        public async Task Delete_Employee_ShouldReturnSuccess_WhenEmployeeExists()
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
            _mock.Setup(x => x.DeleteEmployee(employee.Id));
            var result = _service.DeleteEmployee(employee.Id);
            _mock.Verify(x => x.DeleteEmployee(employee.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployee_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
        {
            var Id = 999;
            // Set up the _mock to throw KeyNotFoundException when DeleteEmployee is called with an invalid ID
            _mock.Setup(x => x.DeleteEmployee(Id))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            // Act & Assert - Expect an exception when calling DeleteEmployee
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteEmployee(Id)
            );

            // Verify the exception message
            Assert.Equal("Employee not found", exception.Message);

            // Ensure DeleteEmployee() was actually called with the given ID
            _mock.Verify(x => x.DeleteEmployee(Id), Times.Once);
        }

        [Fact]
        public async Task Delete_Employee_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
          
            // Set up the _mock to throw an exception when DeleteEmployee is called with the given ID
            _mock.Setup(x => x.DeleteEmployee(Id))
                .ThrowsAsync(new Exception("Unexpected error occurred while deleting employee"));

            // Act & Assert - Expect an exception when calling DeleteEmployee
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.DeleteEmployee(Id)
            );

            // Verify the exception message
            Assert.Equal("Unexpected error occurred while deleting employee", exception.Message);

            // Ensure DeleteEmployee() was actually called with the given ID
            _mock.Verify(x => x.DeleteEmployee(Id), Times.Once);
        }

        [Fact]
        public async Task Get_AllEmployees_ShouldReturnEmployeeList_WhenEmployeesExist()
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
            _mock.Setup(x => x.GetAllEmployees());
            var result = _service.GetAllEmployees();
            Assert.Equal(1, employee.Id);
            Assert.Equal("Raj", employee.FirstName);
            _mock.Verify(x => x.GetAllEmployees(), Times.Once);
        }

        [Fact]
        public async Task Get_AllEmployees_ShouldThrowInvalidOperationException_WhenNoEmployeesAvailable()
        {

            // Set up the _mock to throw an exception when GetAllEmployees() is called
            _mock.Setup(x => x.GetAllEmployees())
                .ThrowsAsync(new InvalidOperationException("No Employee available"));

            // Act & Assert - Expect an exception when calling GetAllEmployees
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetAllEmployees()
            );

            // Verify the exception message
            Assert.Equal("No Employee available", exception.Message);

            // Ensure GetAllEmployees() was actually called
            _mock.Verify(x => x.GetAllEmployees(), Times.Once);
        }

        [Fact]
        public async Task Get_AllEmployees_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
           
            // Set up the _mock to throw an exception when GetAllEmployees() is called
            _mock.Setup(x => x.GetAllEmployees())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert - Expect an exception when calling GetAllEmployees
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetAllEmployees()
            );

            // Verify the exception message
            Assert.Equal("Unexpected error", exception.Message);

            // Ensure GetAllEmployees() was actually called
            _mock.Verify(x => x.GetAllEmployees(), Times.Once);
        }

        [Fact]
        public async Task Get_EmployeeById_ShouldReturnEmployee_WhenEmployeeExists()
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
            _mock.Setup(x => x.GetEmployeeById(1));
            var result = _service.GetEmployeeById(1);
            Assert.NotNull(result);
            Assert.Equal(1, employee.Id);
            Assert.Equal("Raj", employee.FirstName);

        }

        [Fact]
        public async Task Get_EmployeeById_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var Id = 999;
            // Set up the _mock to throw KeyNotFoundException when GetEmployeeById is called with any ID
            _mock.Setup(x => x.GetEmployeeById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            // Act & Assert - Expect an exception when calling GetEmployeeById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetEmployeeById(Id) // Ensure awaited call
            );

            // Verify the exception message
            Assert.Equal("Employee not found", exception.Message);

            // Ensure GetEmployeeById() was actually called with the given ID
            _mock.Verify(x => x.GetEmployeeById(Id), Times.Once);
        }

        [Fact]
        public async Task Get_EmployeeById_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            
            // Set up the _mock to throw an exception when GetEmployeeById is called
            _mock.Setup(x => x.GetEmployeeById(Id))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            // Act & Assert - Expect an exception when calling GetEmployeeById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetEmployeeById(Id)
            );

            // Verify the exception message
            Assert.Equal("Employee not found", exception.Message);

            // Ensure GetEmployeeById() was actually called with the given ID
            _mock.Verify(x => x.GetEmployeeById(Id), Times.Once);
        }

        [Fact]
        public async Task Update_Employee_ShouldReturnSuccess_WhenEmployeeExistsAndValidInputProvided()
        {
            //List<Employee> employees = new List<Employee>();
            //Employee employee = new Employee()
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
            _mock.Setup(x => x.UpdateEmployee(employeeRequestModel)).ReturnsAsync(true);

            var result = await _service.UpdateEmployee(employeeRequestModel);
            Assert.True(result);
            _mock.Verify(x => x.UpdateEmployee(employeeRequestModel), Times.Once);
        }

        [Fact]
        public async Task Update_Employee_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
        {

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
            _mock.Setup(x => x.GetEmployeeById(It.IsAny<int>())).ReturnsAsync((Employee)null);

            // ✅ Ensure UpdateEmployee throws KeyNotFoundException when employee is not found
            _mock.Setup(x => x.UpdateEmployee(It.IsAny<EmployeeRequestModel>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateEmployee(employeeRequestModel)
            );

            // ✅ Verify the exception message
            Assert.Equal("Update not found", exception.Message);
        }

        [Fact]
        public async Task Update_Employee_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
   
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
            _mock.Setup(x => x.GetEmployeeById(It.IsAny<int>())).ReturnsAsync((Employee)null);

            // ✅ Ensure UpdateEmployee throws an exception when null is passed
            _mock.Setup(x => x.UpdateEmployee(null))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateEmployee(null)
            );

            // ✅ Verify the exception message
            Assert.Equal("Update not found", exception.Message);
        }


    }
}
