using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ControllerTests
{
    public class CandidateControllerTest
    {
        private readonly Mock<ICandidateService> _mock;
        private readonly CandidateController _controller;
        public CandidateControllerTest()
        {
            _mock = new Mock<ICandidateService>();
            _controller = new CandidateController(_mock.Object);
        }

        [Fact]
        public void Add_Candidate_WhenValidCandidateProvided_ReturnsSuccess()
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
            _mock.Setup(x=>x.CreateCandidate(It.IsAny<Candidate>())).ReturnsAsync(true);
            var result = _controller.AddCandidate(candidate);
            Assert.NotNull(result);
            _mock.Verify(x=>x.CreateCandidate(It.IsAny<Candidate>()), Times.Once());
        }

        [Fact]
        public void Get_Candidates_WhenCalled_ReturnsCandidateList()
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
            var result = _controller.GetCandidates();
            Assert.NotNull(result);
            _mock.Verify(x => x.GetAllCandidates(), Times.Once());
        }

        [Fact]
        public void Update_Candidate_WhenValidCandidateProvided_UpdatesSuccessfully()
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
            _mock.Setup(x => x.UpdateCandidate(It.IsAny<Candidate>())).ReturnsAsync(true);
            var result = _controller.UpdateCandidate(updatecandidate);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateCandidate(It.Is<Candidate>(r =>
                r.Id == updatecandidate.Id &&
                r.FirstName == updatecandidate.FirstName)), Times.Once());
        }

        [Fact]
        public void Delete_Candidate_WhenCandidateExists_DeletesSuccessfully()
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
            var result = _controller.DeleteCandidate(candidate.Id);
            Assert.NotNull(result);
            _mock.Verify(x => x.DeleteCandidate(candidate.Id), Times.Once);
        }


    }
}
