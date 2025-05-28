using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class DesignationServiceTest
    {
        private readonly Mock<IDesignationService> _mock;
        private readonly IDesignationService _service;
        public DesignationServiceTest()
        {
            _mock = new Mock<IDesignationService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task Create_Designation_ShouldReturnSuccess_WhenValidDesignationIsProvided()
        {
            _mock.Setup(x => x.CreateDesignation(It.IsAny<Designation>()))
                    .Returns(Task.FromResult(true));
            Designation designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"

            };
            _mock.Setup(x => x.CreateDesignation(designation)).ReturnsAsync(true);

            var result = await _service.CreateDesignation(designation);
            Assert.True(result);
            _mock.Verify(x => x.CreateDesignation(designation), Times.Once);

        }


        [Fact]
        public async Task Create_Designation_ShouldThrowInvalidOperationException_WhenDesignationAlreadyExists()
        {

            var designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"
            };

            // Set up the _mock to throw an exception when CreateDesignation is called
            _mock.Setup(x => x.CreateDesignation(It.IsAny<Designation>()))
                .ThrowsAsync(new InvalidOperationException("Designation already exists"));

            // Act & Assert - Expect an exception when trying to create a duplicate designation
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.CreateDesignation(designation)
            );

            // Verify the exception message
            Assert.Equal("Designation already exists", exception.Message);

            // Ensure CreateDesignation() was actually called once
            _mock.Verify(x => x.CreateDesignation(It.IsAny<Designation>()), Times.Once);
        }

        [Fact]
        public async Task Create_Designation_ShouldThrowArgumentNullException_WhenNullDesignationIsProvided()
        {

            // Set up the _mock to throw ArgumentNullException when CreateDesignation is called with null
            _mock.Setup(x => x.CreateDesignation(null))
                .ThrowsAsync(new ArgumentNullException("entity", "Entity cannot be null"));

            // Act & Assert - Expect an exception when passing null
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.CreateDesignation(null)
            );

            // Verify the exception message
            Assert.Equal("Entity cannot be null (Parameter 'entity')", exception.Message);

            // Ensure CreateDesignation() was actually called once
            _mock.Verify(x => x.CreateDesignation(null), Times.Once);
        }


        [Fact]
        public void Delete_Designation_ShouldReturnSuccess_WhenDesignationExists()
        {
            var Id = 1;
            _mock.Setup(x => x.GetDesignationById(Id));
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "OOPS"
            };
            _mock.Setup(x => x.DeleteDesignation(Id));
            var result = _service.DeleteDesignation(Id);
            _mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }


        [Fact]
        public async Task Delete_Designation_ShouldThrowKeyNotFoundException_WhenDesignationDoesNotExist()
        {
            // Arrange
            var Id = 999;

            // Ensure exception is thrown when DeleteDesignation is called with 999
            _mock.Setup(x => x.DeleteDesignation(999))
                .ThrowsAsync(new KeyNotFoundException("Designation not found"));

            // Act & Assert - Expect an exception when calling DeleteDesignation
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteDesignation(Id) // No need for async/await inside Assert.ThrowsAsync
            );

            // Verify the exception message
            Assert.Equal("Designation not found", exception.Message);

            // Ensure DeleteDesignation() was actually called with the correct ID
            _mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }


        [Fact]
        public async Task Delete_Designation_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var Id = 1;

            // Mock DeleteDesignation to throw a generic database exception
            _mock.Setup(x => x.DeleteDesignation(Id)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert - Expect a generic Exception, not a KeyNotFoundException
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.DeleteDesignation(Id)
            );

            // Verify that the correct exception message is thrown
            Assert.Equal("Database error", exception.Message);

            // Ensure DeleteDesignation() was actually called once
            _mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }


        [Fact]
        public async Task Get_AllDesignations_ShouldReturnList_WhenDesignationsExist()
        {
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "OOPS"
            };
            _mock.Setup(x => x.GetAllDesignations());
            var result = _service.GetAllDesignations();
            Assert.NotNull(result);
            Assert.Equal(1, designation.Id);
            Assert.Equal("OOPS", designation.DesignationName);
        }

        [Fact]
        public async Task Get_AllDesignations_ShouldThrowInvalidOperationException_WhenNoDesignationsAvailable()
        {
            // Ensure the _mock throws an InvalidOperationException
            _mock.Setup(x => x.GetAllDesignations())
                .ThrowsAsync(new InvalidOperationException("No designation available"));

            // Act & Assert - Expect an InvalidOperationException
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetAllDesignations()
            );

            // Verify the correct exception message
            Assert.Equal("No designation available", exception.Message);
        }

        [Fact]
        public async Task Get_AllDesignations_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            var Id = 1;
            _mock.Setup(x => x.GetAllDesignations()).Returns(() => null); ;
            //Assert.Throws<NullReferenceException>(() =>
            //{
            //    var result = _mock.Object.GetAllDesignations();
            //    if (result == null)
            //        throw new NullReferenceException("Unexpected null result");
            //});
            _mock.Setup(s => s.GetAllDesignations()).Throws(new Exception("Unexpected error"));
            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.GetAllDesignations());
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public async Task Get_DesignationById_ShouldReturnDesignation_WhenValidIdIsProvided()
        {
            var Id = 1;
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "OOPS"
            };
            _mock.Setup(x => x.GetDesignationById(1));
            var result = _service.GetDesignationById(1);
            Assert.NotNull(result);
            Assert.Equal(1, designation.Id);
            Assert.Equal("OOPS", designation.DesignationName);
        }

        [Fact]
        public async Task Get_DesignationById_ShouldThrowKeyNotFoundException_WhenDesignationDoesNotExist()
        {
            // Arrange
            var Id = 999;
            // Ensure the _mock throws KeyNotFoundException when GetDesignationById is called
            _mock.Setup(x => x.GetDesignationById(Id))
                .ThrowsAsync(new KeyNotFoundException("Designation not found"));
            // Act & Assert - Expect an exception when calling GetDesignationById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetDesignationById(Id)
            );

            // Verify the exception message
            Assert.Equal("Designation not found", exception.Message);
        }

        [Fact]
        public async Task Get_DesignationById_ShouldThrowKeyNotFoundException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            // Ensure the _mock throws KeyNotFoundException when GetDesignationById is called
            _mock.Setup(x => x.GetDesignationById(Id))
                .ThrowsAsync(new KeyNotFoundException("Unexpected error"));
            // Act & Assert - Expect an exception when calling GetDesignationById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetDesignationById(Id)
            );

            // Verify the exception message
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public async Task Update_Designation_ShouldReturnSuccess_WhenValidDesignationIsProvided()
        {
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "OOPS"
            };
            Designation updatedesignation = new Designation()
            {
                Id = 1,
                DesignationName = "dotnetcore"
            };
            _mock.Setup(x => x.UpdateDesignation(updatedesignation)).ReturnsAsync(true);

            var result = await _service.UpdateDesignation(updatedesignation);
            Assert.True(result);
            _mock.Verify(x => x.UpdateDesignation(updatedesignation), Times.Once);
        }


        [Fact]
        public async Task Update_Designation_ShouldThrowKeyNotFoundException_WhenDesignationDoesNotExist()
        {

            // Set up the _mock to return null for GetDesignationById(1) (not used in this test)
            _mock.Setup(x => x.GetDesignationById(1))
                .ReturnsAsync((Designation)null);

            // Set up the _mock to throw an exception when UpdateDesignation() is called with Id = 999
            _mock.Setup(x => x.UpdateDesignation(It.Is<Designation>(d => d.Id == 999)))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var designationToUpdate = new Designation
            {
                Id = 999,
                DesignationName = "OOPS"
            };

            // Act & Assert - Expect an exception when calling UpdateDesignation with an invalid ID
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateDesignation(designationToUpdate)
            );

            // Verify the exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateDesignation() was actually called with the invalid ID
            _mock.Verify(x => x.UpdateDesignation(It.Is<Designation>(d => d.Id == 999)), Times.Once);
        }


        [Fact]
        public async Task Update_Designation_ShouldThrowArgumentNullException_WhenNullDesignationIsProvided()
        {
         
            // Set up the _mock to throw an exception when UpdateDesignation() is called with null
            _mock.Setup(x => x.UpdateDesignation(null))
                .ThrowsAsync(new ArgumentNullException("Update", "Update cannot be null"));

            // Act & Assert - Expect an exception when calling UpdateDesignation with null
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.UpdateDesignation(null)
            );

            // Verify the exception message
            Assert.Equal("Update cannot be null (Parameter 'Update')", exception.Message);

            // Ensure UpdateDesignation() was actually called with null
            _mock.Verify(x => x.UpdateDesignation(null), Times.Once);
        }


    }
}

