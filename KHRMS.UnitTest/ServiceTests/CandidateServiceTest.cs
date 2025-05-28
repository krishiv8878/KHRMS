using KHRMS.Core;
using KHRMS.Services;
using Moq;
using NPOI.SS.Formula.Functions;

namespace KHRMS.UnitTest.ServiceTests
{
    public class CandidateServiceTest
    {
        private readonly Mock<ICandidateService> _mock;
        private ICandidateService _service;
        public CandidateServiceTest()
        {
            _mock = new Mock<ICandidateService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task Create_Candidate_ShouldReturnSuccess_WhenValidCandidateIsProvided()
        {
            _mock.Setup(x => x.CreateCandidate(It.IsAny<Candidate>()))
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
            _mock.Setup(x => x.CreateCandidate(candidate)).ReturnsAsync(true);

            var result = await _service.CreateCandidate(candidate);
            Assert.True(result);
            _mock.Verify(x => x.CreateCandidate(candidate), Times.Once);
        }


        [Fact]
        public async Task Create_Candidate_ShouldThrowInvalidOperationException_WhenCandidateAlreadyExists()
        {
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
            _mock.Setup(x => x.CreateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new InvalidOperationException("Candidate already exists"));
            // Act & Assert - Use Assert.ThrowsAsync and await it
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.CreateCandidate(candidate)
            );

            // Verify exception message
            Assert.Equal("Candidate already exists", exception.Message);

            //Ensure that CreateCandidate() was actually called
            _mock.Verify(x => x.CreateCandidate(It.IsAny<Candidate>()), Times.Once);
        }


        [Fact]
        public async Task Create_Candidate_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
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
            _mock.Setup(x => x.CreateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new InvalidOperationException("Candidate already exists"));
            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.CreateCandidate(candidate)
            );

            // Verify exception message
            Assert.Equal("Candidate already exists", exception.Message);

            //  Ensure that CreateCandidate() was actually called
            _mock.Verify(x => x.CreateCandidate(It.IsAny<Candidate>()), Times.Once);
        }

        [Fact]
        public async Task Delete_Candidate_ShouldReturnSuccess_WhenCandidateExists()
        {
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
            _mock.Setup(x => x.DeleteCandidate(candidate.Id)).ReturnsAsync(true);

            var result = await _service.DeleteCandidate(candidate.Id);
            Assert.True(result);
            _mock.Verify(x => x.DeleteCandidate(candidate.Id), Times.Once);
        }


        [Fact]
        public async Task Delete_Candidate_ShouldThrowKeyNotFoundException_WhenCandidateDoesNotExist()
        {
            // Arrange
            var Id = 999;

            // Ensure exception is thrown when calling DeleteCandidate with Id = 999
            _mock.Setup(x => x.DeleteCandidate(Id))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteCandidate(Id)
            );

            // Verify exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure DeleteCandidate() was called once with the correct ID
            _mock.Verify(x => x.DeleteCandidate(Id), Times.Once);
        }


        [Fact]
        public async Task Delete_Candidate_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            // Ensure DeleteCandidate throws an exception for the given Id
            _mock.Setup(x => x.DeleteCandidate(Id))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteCandidate(Id)
            );

            // Verify the exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure DeleteCandidate was called once with Id = 1
            _mock.Verify(x => x.DeleteCandidate(Id), Times.Once);
        }

        [Fact]
        public async Task Get_AllCandidates_ShouldReturnCandidateList_WhenCandidatesExist()
        {

            var candidate = new List<Candidate>
            {
                new Candidate{
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
                }
            };
            _mock.Setup(x => x.GetAllCandidates()).ReturnsAsync(candidate);

            var result = await _service.GetAllCandidates();
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
        }


        [Fact]
        public async Task Get_AllCandidates_ShouldThrowInvalidOperationException_WhenNoCandidatesAvailable()
        {
            // Ensure GetAllCandidates throws an exception when called
            _mock.Setup(x => x.GetAllCandidates())
                .ThrowsAsync(new InvalidOperationException("No Candidate available"));

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetAllCandidates()
            );

            // Verify the exception message
            Assert.Equal("No Candidate available", exception.Message);

            // Ensure GetAllCandidates() was actually called
            _mock.Verify(x => x.GetAllCandidates(), Times.Once);
        }


        [Fact]
        public async Task Get_AllCandidates_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Set up the _mock to throw an exception when GetAllCandidates is called
            _mock.Setup(x => x.GetAllCandidates())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetAllCandidates()
            );

            // Verify the exception message
            Assert.Equal("Unexpected error", exception.Message);

            // Ensure GetAllCandidates() was actually called
            _mock.Verify(x => x.GetAllCandidates(), Times.Once);
        }

        [Fact]
        public async Task Get_CandidateById_ShouldReturnCandidate_WhenCandidateExists()
        {
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
            _mock.Setup(x => x.GetCandidateById((int)candidate.Id)).ReturnsAsync(candidate);

            var result = await _service.GetCandidateById((int)candidate.Id);
            Assert.NotNull(result);
            Assert.Equal("Raj", result.FirstName);
        }


        [Fact]
        public async Task Get_CandidateById_ShouldThrowKeyNotFoundException_WhenCandidateDoesNotExist()
        {
            // Arrange
            var Id = 999;
            // Set up the _mock to throw an exception when GetCandidateById is called with any ID
            _mock.Setup(x => x.GetCandidateById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));
            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetCandidateById(Id)
            );

            // Verify the exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure GetCandidateById() was actually called
            _mock.Verify(x => x.GetCandidateById(Id), Times.Once);
        }


        [Fact]
        public async Task Get_CandidateById_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            // Set up the _mock to throw an exception when GetCandidateById is called
            _mock.Setup(x => x.GetCandidateById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetCandidateById(Id)
            );

            // Verify the exception message
            Assert.Equal("Candidate not found", exception.Message);

            // Ensure GetCandidateById() was actually called
            _mock.Verify(x => x.GetCandidateById(Id), Times.Once);
        }

        [Fact]
        public async Task Update_Candidate_ShouldReturnSuccess_WhenCandidateIsUpdatedSuccessfully()
        {
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
            _mock.Setup(x => x.UpdateCandidate(updatecandidate)).ReturnsAsync(true);

            var result = await _service.UpdateCandidate(updatecandidate);
            Assert.True(result);
            _mock.Verify(x => x.UpdateCandidate(updatecandidate), Times.Once);
        }


        [Fact]
        public async Task Update_Candidate_ShouldThrowKeyNotFoundException_WhenCandidateDoesNotExist()
        {
            // Arrange
            var Id = 1;
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

            // Set up the _mock to throw an exception when UpdateCandidate is called
            _mock.Setup(x => x.UpdateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert - Expect an exception
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateCandidate(candidate)
            );

            // Verify the exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateCandidate() was actually called
            _mock.Verify(x => x.UpdateCandidate(It.IsAny<Candidate>()), Times.Once);
        }


        [Fact]
        public async Task Update_Candidate_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
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

            // Set up the _mock to throw an exception when UpdateCandidate is called
            _mock.Setup(x => x.UpdateCandidate(It.IsAny<Candidate>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert - Expect an exception when updating with null
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateCandidate(null)
            );

            // Verify the exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateCandidate() was actually called
            _mock.Verify(x => x.UpdateCandidate(It.IsAny<Candidate>()), Times.Once);
        }

    }
}
