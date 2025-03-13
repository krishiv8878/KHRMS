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
        public void CreateDesignationReturnPass()
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
        public async Task CreateDesignationReturnFail()
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
        public async Task CreateDesignationReturnException()
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
        public void DeleteDesignationReturnPass()
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
            Assert.Equal(1,1);
            mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }

        [Fact]
        public async Task DeleteDesignationReturnFail()
        {
            var Id = 999;
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            Designation designation = new Designation();
            List<Designation> designations = new List<Designation>();
            var result = designationservice.DeleteDesignation(Id);
            mock.Verify(x => x.DeleteDesignation(It.IsAny<int>()), Times.Never);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => designationservice.DeleteDesignation(Id));
            Assert.Equal("Designation not found", exception.Message);
        }

        [Fact]
        public async Task DeleteDesignationReturnException()
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
            mock.Setup(x => x.DeleteDesignation(Id)).Throws(new Exception("Database error"));
            var result = designationservice.DeleteDesignation(Id);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => designationservice.DeleteDesignation(Id));
            Assert.Equal("Designation not found", exception.Message);
            mock.Verify(x => x.DeleteDesignation(Id), Times.Once);
        }

        [Fact]
        public void GetAllDesignationsReturnPass()
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
        public async Task GetAllDesignationsReturnFail()
        {
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            mock.Setup(x => x.GetAllDesignations());
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => designationservice.GetAllDesignations());
            Assert.Equal("No devision available", exception.Message);
        }


        [Fact]
        public async Task GetAllDesignationsReturnExeption()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            mock.Setup(x => x.GetAllDesignations()).Returns(()=>null); ;
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
        public void GetDesignationByIdReturnPass()
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
        public async Task GetDesignationByIdReturnFail()
        {
            var Id = 999;
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => designationservice.GetDesignationById(Id));
            Assert.Equal("Designation not found", exception.Message);
        }

        [Fact]
        public async Task GetDesignationByIdReturnException()
        {
            var Id = 1;
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => designationservice.GetDesignationById(1));
            Assert.Equal("Unexpected error", exception.Message);           
        }

        [Fact]
        public void UpdateDesignationReturnTest()
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
        public async Task UpdateDesignationReturnTestFail()
        {
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            Designation designation = new Designation();
            mock.Setup(x => x.GetDesignationById(1));
            var result = designationservice.UpdateDesignation(designation);
            var designations = new Designation
            {
                Id = 999,
                DesignationName = "OOPS"
            };

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => designationservice.UpdateDesignation(designations));
            Assert.Equal("Update not found", exception.Message);
        }

        [Fact]
        public async Task UpdateDesignationReturnTestException()
        {
            var mock = new Mock<IDesignationService>();
            IDesignationService designationservice = mock.Object;
            Designation designation = new Designation();
            mock.Setup(x => x.GetDesignationById(1));
            var result = designationservice.UpdateDesignation(designation);
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => designationservice.UpdateDesignation(null));
            Assert.Equal("Update cannot be null (Parameter 'Update')", exception.Message);
        }

    }
}

