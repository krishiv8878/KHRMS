
using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class EmployeeDocumentServiceTest
    {
        private readonly Mock<IEmployeeDocumentService> _mock;
        public EmployeeDocumentServiceTest()
        {
            _mock = new Mock<IEmployeeDocumentService>();
        }
        [Fact]
        public async Task Document_AddSuccessfully()
        {
            var employeeDocument = new EmployeeDocumentInfo()
            {
                Id = 1,
                EmployeeId = 1,
                FilePath = "C:/user/user/docs",
                UploadedBy = 1,
                UploadedDate = DateTime.Now,
            };
            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>())).Returns(Task.CompletedTask);
            await _mock.Object.AddAsync(employeeDocument);
            _mock.Verify(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>()), Times.Once);
        }
        [Fact]
        public async Task EmployeeDocument_ThrowException_whenDataInvalid()
        {
            var employeeDocument = new EmployeeDocumentInfo()
            {
                Id = 1,
                EmployeeId = 1,
                FilePath = "C://user//user//docs",//file path is wrong
                UploadedBy = 1,
                UploadedDate = DateTime.Now,
            };

            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>())).Throws(new ArgumentException("Invalid Data of EmployeeDocument"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.AddAsync(employeeDocument));
            Assert.Equal("Invalid Data of EmployeeDocument", exception.Message);
        }
        [Fact]
        public async Task EmployeeDocument_ThrowException_whenDataIsNull()
        {
            EmployeeDocumentInfo employeeDocument = null;

            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>())).Throws(new Exception("Document Data Is Null"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.AddAsync(employeeDocument));

            Assert.Equal("Document Data Is Null", exception.Message);
        }
        [Fact]
        public async Task EmployeeDocument_GetSuccessfully()
        {
            var employeeDocument = new List<EmployeeDocumentInfo>
            {
                new EmployeeDocumentInfo() {Id = 1,EmployeeId = 1,FilePath="C:/user/users/docs",UploadedBy=1,UploadedDate=DateTime.Now},
                new EmployeeDocumentInfo() {Id = 2,EmployeeId = 2,FilePath="C:/user/users/docs",UploadedBy=1,UploadedDate=DateTime.Now}
            };
            _mock.Setup(x => x.GetAllAsync()).ReturnsAsync(employeeDocument);
            var result = await _mock.Object.GetAllAsync();

            _mock.Verify(x => x.GetAllAsync(), Times.Once);

            Assert.Equal(employeeDocument.Count(), result.Count());
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);
        }
        [Fact]
        public async Task EmployeeDocument_NotFound()
        {
            var employeeDocument = new List<EmployeeDocumentInfo>();
            _mock.Setup(x => x.GetAllAsync()).ThrowsAsync(new Exception("Employee Document Not Found"));
            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.GetAllAsync());
            Assert.Equal("Employee Document Not Found", exception.Message);
        }
        [Fact]
        public async Task EmployeeDocument_GetSuccessFullyById()
        {
            var employeeDocumentid = 1;
            var employeeDocument = new EmployeeDocumentInfo { Id = employeeDocumentid };
            _mock.Setup(x => x.GetByIdAsync(employeeDocumentid)).ReturnsAsync(employeeDocument);
            await _mock.Object.GetByIdAsync(employeeDocumentid);
            _mock.Verify(x => x.GetByIdAsync(employeeDocumentid), Times.Once());
        }
        [Fact]
        public async Task EmployeeDocument_IdNotFound()
        {
            var employeeDocumentid = 1;
            var employeeDocument = new EmployeeDocumentInfo { Id = employeeDocumentid };
            _mock.Setup(x => x.GetByIdAsync(employeeDocumentid)).ThrowsAsync(new ArgumentException("EmployeeDocumentId Not found"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.GetByIdAsync(employeeDocumentid));
            Assert.Equal("EmployeeDocumentId Not found", exception.Message);
        }
        [Fact]
        public async Task EmployeeDocument_DeleteSuccessfully()
        {
            var employeeDocumentid = 1;
            _mock.Setup(x => x.DeleteAsync(employeeDocumentid)).Returns(Task.CompletedTask);
            await _mock.Object.DeleteAsync(employeeDocumentid);
            _mock.Verify(x => x.DeleteAsync(employeeDocumentid), Times.Once);
        }
        [Fact]
        public async Task EmployeeDocument_DeleteThrowException_WhenNotFound()
        {
            var employeeDocumentid = 1;
            _mock.Setup(x => x.DeleteAsync(employeeDocumentid)).ThrowsAsync(new KeyNotFoundException("Employee Document Not found"));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.DeleteAsync(employeeDocumentid));
            Assert.Equal("Employee Document Not found", exception.Message);
        }
    }
}
