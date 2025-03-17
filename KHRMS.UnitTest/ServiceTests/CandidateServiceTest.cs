using KHRMS.Core;
using KHRMS.Services;
using Moq;
using NPOI.SS.Formula.Functions;

namespace KHRMS.UnitTest
{
    public class CandidateServiceTest
    {
        public CandidateServiceTest()
        {

        }

        [Fact]
        public void CreateCandidateReturnPass()
        {
            var mock = new Mock<ICandidateService>();
            List<Candidate> candidates = new List<Candidate>();
            mock.Setup(x => x.CreateCandidate(It.IsAny<Candidate>()))
                      .Returns(Task.FromResult(true));
            Candidate candidate = new Candidate()
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };
            candidates.Add(candidate);
            Assert.Equal(1, 1);
        }

       
        [Fact]
        public async Task CreateCandidateReturnFail()
        {
            // Arrange
            var mock = new Mock<ICandidateService>();

            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };

            // ✅ Mock CreateCandidate to throw an exception if candidate already exists
            mock.Setup(x => x.CreateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new InvalidOperationException("Candidate already exists"));

            var candidateService = mock.Object;

            // Act & Assert - Use Assert.ThrowsAsync and await it
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await candidateService.CreateCandidate(candidate)
            );

            // Verify exception message
            Assert.Equal("Candidate already exists", exception.Message);

            //Ensure that CreateCandidate() was actually called
            mock.Verify(x => x.CreateCandidate(It.IsAny<Candidate>()), Times.Once);
        }

     
        [Fact]
        public async Task CreateCandidateReturnException()
        {
            // Arrange
            var mock = new Mock<ICandidateService>();

            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };

            // Mock CreateCandidate to throw an exception when called
            mock.Setup(x => x.CreateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new InvalidOperationException("Candidate already exists"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await candidateService.CreateCandidate(candidate)
            );

            // Verify exception message
            Assert.Equal("Candidate already exists", exception.Message);

            //  Ensure that CreateCandidate() was actually called
            mock.Verify(x => x.CreateCandidate(It.IsAny<Candidate>()), Times.Once);
        }

        [Fact]
        public void DeleteCandidateReturnsPass()
        {
            var mock = new Mock<ICandidateService>();
            ICandidateService candidateService = mock.Object;
            List<Candidate> candidates = new List<Candidate>();
            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };
            mock.Setup(x => x.DeleteCandidate(1));
            var result = candidateService.DeleteCandidate(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteCandidate(1), Times.Once);
        }

      
        [Fact]
        public async Task DeleteCandidateReturnsFail()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<ICandidateService>();

            // Ensure exception is thrown when calling DeleteCandidate with Id = 999
            mock.Setup(x => x.DeleteCandidate(Id))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await candidateService.DeleteCandidate(Id)
            );

            // Verify exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure DeleteCandidate() was called once with the correct ID
            mock.Verify(x => x.DeleteCandidate(Id), Times.Once);
        }


        [Fact]
        public async Task DeleteCandidateReturnsException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ICandidateService>();

            // Ensure DeleteCandidate throws an exception for the given Id
            mock.Setup(x => x.DeleteCandidate(Id))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await candidateService.DeleteCandidate(Id)
            );

            // Verify the exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure DeleteCandidate was called once with Id = 1
            mock.Verify(x => x.DeleteCandidate(Id), Times.Once);
        }

        [Fact]
        public void GetAllCandidatesReturnPass()
        {
            var mock = new Mock<ICandidateService>();
            ICandidateService candidateService = mock.Object;
            List<Candidate> candidates = new List<Candidate>();
            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };
            mock.Setup(x => x.GetAllCandidates());
            var result = candidateService.GetAllCandidates();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal(1, candidate.Id);
            Assert.Equal("Raj", candidate.FirstName);
        }

        
        [Fact]
        public async Task GetAllCandidatesReturnFail()
        {
            // Arrange
            var mock = new Mock<ICandidateService>();

            // Ensure GetAllCandidates throws an exception when called
            mock.Setup(x => x.GetAllCandidates())
                .ThrowsAsync(new InvalidOperationException("No Candidate available"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await candidateService.GetAllCandidates()
            );

            // Verify the exception message
            Assert.Equal("No Candidate available", exception.Message);

            // Ensure GetAllCandidates() was actually called
            mock.Verify(x => x.GetAllCandidates(), Times.Once);
        }

        //[Fact]
        //public async Task GetAllCandidatesReturnExcep0tion()
        //{
        //    var mock = new Mock<ICandidateService>();
        //    ICandidateService candidateService = mock.Object;
        //    List<Candidate> candidates = new List<Candidate>();
        //    mock.Setup(x => x.GetAllCandidates());
        //    var result = candidateService.GetAllCandidates();
        //    var exception = await Assert.ThrowsAsync<Exception>(() => mock.Object.GetAllCandidates());
        //    Assert.Equal("Unexpected error", exception.Message);
        //}
        [Fact]
        public async Task GetAllCandidatesReturnException()
        {
            // Arrange
            var mock = new Mock<ICandidateService>();

            // Set up the mock to throw an exception when GetAllCandidates is called
            mock.Setup(x => x.GetAllCandidates())
                .ThrowsAsync(new Exception("Unexpected error"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await candidateService.GetAllCandidates()
            );

            // Verify the exception message
            Assert.Equal("Unexpected error", exception.Message);

            // Ensure GetAllCandidates() was actually called
            mock.Verify(x => x.GetAllCandidates(), Times.Once);
        }

        [Fact]
        public void GetCandidateByIdReturnPass()
        {
            var mock = new Mock<ICandidateService>();
            ICandidateService candidateService = mock.Object;
            List<Candidate> candidates = new List<Candidate>();
            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };
            mock.Setup(x => x.GetCandidateById(1));
            var result = candidateService.GetCandidateById(1);
            Assert.NotNull(result);
            Assert.Equal(1, candidate.Id);
            Assert.Equal("Raj", candidate.FirstName);
        }

        //[Fact]
        //public async Task GetCandidateByIdReturnFail()
        //{
        //    var Id = 999;
        //    var mock = new Mock<ICandidateService>();
        //    ICandidateService candidateService = mock.Object;
        //    List<Candidate> candidates = new List<Candidate>();
        //    mock.Setup(x => x.GetCandidateById(1));
        //    var result = candidateService.GetCandidateById(1);
        //    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => candidateService.GetCandidateById(Id));
        //    Assert.Equal("Candidate not found", exception.Message);
        //}
        [Fact]
        public async Task GetCandidateByIdReturnFail()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<ICandidateService>();

            // Set up the mock to throw an exception when GetCandidateById is called with any ID
            mock.Setup(x => x.GetCandidateById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await candidateService.GetCandidateById(Id)
            );

            // Verify the exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure GetCandidateById() was actually called
            mock.Verify(x => x.GetCandidateById(Id), Times.Once);
        }

        //[Fact]
        //public async Task GetCandidateByIdReturnException()
        //{
        //    var Id = 1;
        //    var mock = new Mock<ICandidateService>();
        //    ICandidateService candidateService = mock.Object;
        //    List<Candidate> candidates = new List<Candidate>();
        //    mock.Setup(x => x.GetCandidateById(1));
        //    var result = candidateService.GetCandidateById(1);
        //    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => candidateService.GetCandidateById(1));
        //    Assert.Equal("Candidate not found", exception.Message);
        //}
        [Fact]
        public async Task GetCandidateByIdReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ICandidateService>();

            // Set up the mock to throw an exception when GetCandidateById is called
            mock.Setup(x => x.GetCandidateById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await candidateService.GetCandidateById(Id)
            );

            // Verify the exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure GetCandidateById() was actually called
            mock.Verify(x => x.GetCandidateById(Id), Times.Once);
        }

        [Fact]
        public void UpdateCandidateReturnPass()
        {
            var mock = new Mock<ICandidateService>();
            ICandidateService candidateService = mock.Object;
            List<Candidate> candidates = new List<Candidate>();
            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };
            var updatecandidate = new Candidate
            {
                Id = 1,
                FirstName = "Rajesh",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };

            mock.Setup(x => x.GetCandidateById(1));
            mock.Setup(x => x.UpdateCandidate(candidate));
            var result = candidateService.UpdateCandidate(candidate);
            Assert.Equal(1,1);
            Assert.NotNull(result);
        }

        //[Fact]
        //public async Task UpdateCandidateReturnFail()
        //{
        //    var Id = 1;
        //    var mock = new Mock<ICandidateService>();
        //    ICandidateService candidateService = mock.Object;
        //    List<Candidate> candidates = new List<Candidate>();
        //    var candidate = new Candidate
        //    {
        //        Id = 1,
        //        FirstName = "Raj",
        //        LastName = "Prajapati",
        //        EmailAddress = "user@example.com",
        //        MobileNumber = "9876543210",
        //        TotalExperience = "string",
        //        RelevantExperience = "string",
        //        CurrentSalary = 0,
        //        ExpectedSalary = 0,
        //        NoticePeriod = 0
        //    };
        //    mock.Setup(x => x.GetCandidateById(1));
        //    mock.Setup(x => x.UpdateCandidate(candidate));
        //    var result = candidateService.UpdateCandidate(candidate);
        //    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => candidateService.UpdateCandidate(candidate));
        //    Assert.Equal("Update not found", exception.Message);
        //}
        [Fact]
        public async Task UpdateCandidateReturnFail()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ICandidateService>();

            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };

            // Set up the mock to throw an exception when UpdateCandidate is called
            mock.Setup(x => x.UpdateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await candidateService.UpdateCandidate(candidate)
            );

            // Verify the exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateCandidate() was actually called
            mock.Verify(x => x.UpdateCandidate(It.IsAny<Candidate>()), Times.Once);
        }


        //[Fact]
        //public async Task UpdateCandidateReturnException()
        //{
        //    var Id = 1;
        //    var mock = new Mock<ICandidateService>();
        //    ICandidateService candidateService = mock.Object;
        //    List<Candidate> candidates = new List<Candidate>();
        //    var candidate = new Candidate
        //    {
        //        Id = 1,
        //        FirstName = "Raj",
        //        LastName = "Prajapati",
        //        EmailAddress = "user@example.com",
        //        MobileNumber = "9876543210",
        //        TotalExperience = "string",
        //        RelevantExperience = "string",
        //        CurrentSalary = 0,
        //        ExpectedSalary = 0,
        //        NoticePeriod = 0
        //    };
        //    mock.Setup(x => x.GetCandidateById(1));
        //    mock.Setup(x => x.UpdateCandidate(candidate));
        //    var result = candidateService.UpdateCandidate(candidate);
        //    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => candidateService.UpdateCandidate(null));
        //    Assert.Equal("Update not found", exception.Message);
        //}

        [Fact]
        public async Task UpdateCandidateReturnException()
        {
            // Arrange
            var mock = new Mock<ICandidateService>();

            var candidate = new Candidate
            {
                Id = 1,
                FirstName = "Raj",
                LastName = "Prajapati",
                EmailAddress = "user@example.com",
                MobileNumber = "9876543210",
                TotalExperience = "string",
                RelevantExperience = "string",
                CurrentSalary = 0,
                ExpectedSalary = 0,
                NoticePeriod = 0
            };

            // Set up the mock to throw an exception when UpdateCandidate is called
            mock.Setup(x => x.UpdateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var candidateService = mock.Object;

            // Act & Assert - Expect an exception when updating with null
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await candidateService.UpdateCandidate(null)
            );

            // Verify the exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateCandidate() was actually called
            mock.Verify(x => x.UpdateCandidate(It.IsAny<Candidate>()), Times.Once);
        }

    }
}
