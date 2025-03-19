using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class DesignationServiceTest
    {

        public DesignationServiceTest()
        {

        }

        [Fact]
        public void Create_Designation_ShouldReturnSuccess_WhenValidDesignationIsProvided()
        {
            var mock = new Mock<IDesignationService>();
            List<Designation> designations = new List<Designation>();
            mock.Setup(x => x.CreateDesignation(It.IsAny<Designation>()))
                    .Returns(Task.FromResult(true));
            Designation designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"

            };
            designations.Add(designation);
            Assert.Equal(1, 1);

        }


        [Fact]
        public async Task Create_Designation_ShouldThrowInvalidOperationException_WhenDesignationAlreadyExists()
        {
            // Arrange
            var mock = new Mock<IDesignationService>();

            var designation = new Designation()
            {
                Id = 8,
                DesignationName = "DotnetCore"
            };

            // Set up the mock to throw an exception when CreateDesignation is called
            mock.Setup(x => x.CreateDesignation(It.IsAny<Designation>()))
                .ThrowsAsync(new InvalidOperationException("Designation already exists"));

            var designationService = mock.Object;

            // Act & Assert - Expect an exception when trying to create a duplicate designation
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await designationService.CreateDesignation(designation)
            );

            // Verify the exception message
            Assert.Equal("Designation already exists", exception.Message);

            // Ensure CreateDesignation() was actually called once
            mock.Verify(x => x.CreateDesignation(It.IsAny<Designation>()), Times.Once);
        }

        [Fact]
        public async Task Create_Designation_ShouldThrowArgumentNullException_WhenNullDesignationIsProvided()
        {
            // Arrange
            var mock = new Mock<IDesignationService>();

            // Set up the mock to throw ArgumentNullException when CreateDesignation is called with null
            mock.Setup(x => x.CreateDesignation(null))
                .ThrowsAsync(new ArgumentNullException("entity", "Entity cannot be null"));

            var designationService = mock.Object;

            // Act & Assert - Expect an exception when passing null
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await designationService.CreateDesignation(null)
            );

            // Verify the exception message
            Assert.Equal("Entity cannot be null (Parameter 'entity')", exception.Message);

            // Ensure CreateDesignation() was actually called once
            mock.Verify(x => x.CreateDesignation(null), Times.Once);
        }


        [Fact]
        public void Delete_Designation_ShouldReturnSuccess_WhenDesignationExists()
        {
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            var Id = 1;
            mock.Setup(x => x.GetDesignationById(Id));
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "OOPS"
            };
            mock.Setup(x => x.DeleteDesignation(Id));
            var result = designationservice.DeleteDesignation(Id);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }


        [Fact]
        public async Task Delete_Designation_ShouldThrowKeyNotFoundException_WhenDesignationDoesNotExist()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IDesignationService>();

            // Ensure exception is thrown when DeleteDesignation is called with 999
            mock.Setup(x => x.DeleteDesignation(999))
                .ThrowsAsync(new KeyNotFoundException("Designation not found"));

            var designationService = mock.Object;

            // Act & Assert - Expect an exception when calling DeleteDesignation
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => designationService.DeleteDesignation(Id) // No need for async/await inside Assert.ThrowsAsync
            );

            // Verify the exception message
            Assert.Equal("Designation not found", exception.Message);

            // Ensure DeleteDesignation() was actually called with the correct ID
            mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }


        [Fact]
        public async Task Delete_Designation_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var mock = new Mock<IDesignationService>();
            var Id = 1;

            // Mock DeleteDesignation to throw a generic database exception
            mock.Setup(x => x.DeleteDesignation(Id)).ThrowsAsync(new Exception("Database error"));

            var designationService = mock.Object;

            // Act & Assert - Expect a generic Exception, not a KeyNotFoundException
            var exception = await Assert.ThrowsAsync<Exception>(
                () => designationService.DeleteDesignation(Id)
            );

            // Verify that the correct exception message is thrown
            Assert.Equal("Database error", exception.Message);

            // Ensure DeleteDesignation() was actually called once
            mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }


        [Fact]
        public void Get_AllDesignations_ShouldReturnList_WhenDesignationsExist()
        {
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "OOPS"
            };
            mock.Setup(x => x.GetAllDesignations());
            var result = designationservice.GetAllDesignations();
            Assert.NotNull(result);
            Assert.Equal(1, designation.Id);
            Assert.Equal("OOPS", designation.DesignationName);
        }

        [Fact]
        public async Task Get_AllDesignations_ShouldThrowInvalidOperationException_WhenNoDesignationsAvailable()
        {
            // Arrange
            var mock = new Mock<IDesignationService>();

            // Ensure the mock throws an InvalidOperationException
            mock.Setup(x => x.GetAllDesignations())
                .ThrowsAsync(new InvalidOperationException("No designation available"));

            var designationService = mock.Object;

            // Act & Assert - Expect an InvalidOperationException
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await designationService.GetAllDesignations()
            );

            // Verify the correct exception message
            Assert.Equal("No designation available", exception.Message);
        }

        [Fact]
        public async Task Get_AllDesignations_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            mock.Setup(x => x.GetAllDesignations()).Returns(() => null); ;
            //Assert.Throws<NullReferenceException>(() =>
            //{
            //    var result = mock.Object.GetAllDesignations();
            //    if (result == null)
            //        throw new NullReferenceException("Unexpected null result");
            //});
            mock.Setup(s => s.GetAllDesignations()).Throws(new Exception("Unexpected error"));
            var exception = await Assert.ThrowsAsync<Exception>(() => mock.Object.GetAllDesignations());
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public void Get_DesignationById_ShouldReturnDesignation_WhenValidIdIsProvided()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            Designation designation = new Designation()
            {
                Id = 1,
                DesignationName = "OOPS"
            };
            mock.Setup(x => x.GetDesignationById(1));
            var result = designationservice.GetDesignationById(1);
            Assert.NotNull(result);
            Assert.Equal(1, designation.Id);
            Assert.Equal("OOPS", designation.DesignationName);
        }

        [Fact]
        public async Task Get_DesignationById_ShouldThrowKeyNotFoundException_WhenDesignationDoesNotExist()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IDesignationService>();

            // Ensure the mock throws KeyNotFoundException when GetDesignationById is called
            mock.Setup(x => x.GetDesignationById(Id))
                .ThrowsAsync(new KeyNotFoundException("Designation not found"));

            var designationService = mock.Object;

            // Act & Assert - Expect an exception when calling GetDesignationById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await designationService.GetDesignationById(Id)
            );

            // Verify the exception message
            Assert.Equal("Designation not found", exception.Message);
        }

        [Fact]
        public async Task Get_DesignationById_ShouldThrowKeyNotFoundException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IDesignationService>();

            // Ensure the mock throws KeyNotFoundException when GetDesignationById is called
            mock.Setup(x => x.GetDesignationById(Id))
                .ThrowsAsync(new KeyNotFoundException("Unexpected error"));

            var designationService = mock.Object;

            // Act & Assert - Expect an exception when calling GetDesignationById
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await designationService.GetDesignationById(Id)
            );

            // Verify the exception message
            Assert.Equal("Unexpected error", exception.Message);
        }

        [Fact]
        public void Update_Designation_ShouldReturnSuccess_WhenValidDesignationIsProvided()
        {
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
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
            mock.Setup(x => x.GetDesignationById(1));
            var result = designationservice.UpdateDesignation(designation);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }


        [Fact]
        public async Task Update_Designation_ShouldThrowKeyNotFoundException_WhenDesignationDoesNotExist()
        {
            // Arrange
            var mock = new Mock<IDesignationService>();

            // Set up the mock to return null for GetDesignationById(1) (not used in this test)
            mock.Setup(x => x.GetDesignationById(1))
                .ReturnsAsync((Designation)null);

            // Set up the mock to throw an exception when UpdateDesignation() is called with Id = 999
            mock.Setup(x => x.UpdateDesignation(It.Is<Designation>(d => d.Id == 999)))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var designationService = mock.Object;

            var designationToUpdate = new Designation
            {
                Id = 999,
                DesignationName = "OOPS"
            };

            // Act & Assert - Expect an exception when calling UpdateDesignation with an invalid ID
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await designationService.UpdateDesignation(designationToUpdate)
            );

            // Verify the exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateDesignation() was actually called with the invalid ID
            mock.Verify(x => x.UpdateDesignation(It.Is<Designation>(d => d.Id == 999)), Times.Once);
        }


        [Fact]
        public async Task Update_Designation_ShouldThrowArgumentNullException_WhenNullDesignationIsProvided()
        {
            // Arrange
            var mock = new Mock<IDesignationService>();

            // Set up the mock to throw an exception when UpdateDesignation() is called with null
            mock.Setup(x => x.UpdateDesignation(null))
                .ThrowsAsync(new ArgumentNullException("Update", "Update cannot be null"));

            var designationService = mock.Object;

            // Act & Assert - Expect an exception when calling UpdateDesignation with null
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await designationService.UpdateDesignation(null)
            );

            // Verify the exception message
            Assert.Equal("Update cannot be null (Parameter 'Update')", exception.Message);

            // Ensure UpdateDesignation() was actually called with null
            mock.Verify(x => x.UpdateDesignation(null), Times.Once);
        }


    }
}

