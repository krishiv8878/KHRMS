using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class CandidateControllerTest
    {
        public CandidateControllerTest()
        {

        }


        [Fact]
        public void Add_Candidate_WhenValidCandidateProvided_ReturnsSuccess()
        {
            var mock = new Mock<ICandidateService>();
            mock.Setup(x => x.CreateCandidate(It.IsAny<Candidate>()));
            var controller = new CandidateController(mock.Object);
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
            var result = controller.AddCandidate(candidate);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("Raj", candidate.FirstName);
        }

        [Fact]
        public void Get_Candidates_WhenCalled_ReturnsCandidateList()
        {
            var Id = 1;
            var mock = new Mock<ICandidateService>();
            mock.Setup(x => x.GetAllCandidates());
            var controller = new CandidateController(mock.Object);
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
            var result = controller.GetCandidates();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("Raj", candidate.FirstName);
        }

        [Fact]
        public void Update_Candidate_WhenValidCandidateProvided_UpdatesSuccessfully()
        {
            var Id = 1;
            var mock = new Mock<ICandidateService>();
            // mock.Setup(x => x.UpdateCandidate());
            var controller = new CandidateController(mock.Object);
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
                FirstName = "Anil",
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
            var result = controller.UpdateCandidate(candidate);
            Assert.NotNull(result);
            Assert.Equal(1, candidate.Id);
            Assert.Equal("Raj", candidate.FirstName);
        }

        [Fact]
        public void Delete_Candidate_WhenCandidateExists_DeletesSuccessfully()
        {
            var Id = 1;
            var mock = new Mock<ICandidateService>();
            mock.Setup(x => x.GetAllCandidates());
            var controller = new CandidateController(mock.Object);
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
            var result = controller.DeleteCandidate(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteCandidate(1), Times.Once);
        }


    }
}
