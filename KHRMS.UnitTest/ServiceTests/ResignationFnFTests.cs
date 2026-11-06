using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class ResignationFnFTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IResignationRepository> _resRepoMock;
        private readonly Mock<ISendEmailService> _emailMock;
        private readonly Mock<IUserContextService> _userContextMock;
        private readonly Mock<IChecklistService> _checklistMock;
        private readonly ResignationService _service;

        public ResignationFnFTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _resRepoMock = new Mock<IResignationRepository>();
            _emailMock = new Mock<ISendEmailService>();
            _userContextMock = new Mock<IUserContextService>();
            _checklistMock = new Mock<IChecklistService>();

            _userContextMock.Setup(u => u.IsAdmin()).Returns(true);
            _userContextMock.Setup(u => u.GetCurrentEmployeeId()).Returns(100L);

            _uowMock.Setup(u => u.Resignation).Returns(_resRepoMock.Object);
            _uowMock.Setup(u => u.Save()).Returns(1);

            _service = new ResignationService(
                _uowMock.Object,
                _emailMock.Object,
                _userContextMock.Object,
                _checklistMock.Object);
        }

        [Fact]
        public async Task UpdateFnFStatusAsync_WhenPendingAssetsAndNoOverride_ThrowsInvalidOperationException()
        {
            var res = new Resignation
            {
                Id = 1,
                EmployeeId = 50,
                ResignationStatus = "ExitClearance",
                FnFStatus = "Pending",
                IsDeleted = false
            };

            _resRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(res);

            _checklistMock.Setup(c => c.VerifyAssetReturnsAsync(50))
                .ReturnsAsync(new AssetReturnVerificationDto
                {
                    AllCleared = false,
                    PendingAssets = 2,
                    PendingAssetDetails = new List<string> { "MacBook Pro (AST-001)", "Access Card (AST-042)" }
                });

            var dto = new FnFStatusUpdateDto
            {
                Status = "Prepared",
                Remarks = "Ready for payout"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateFnFStatusAsync(1, dto));
            Assert.Contains("pending return", ex.Message);
            Assert.Contains("MacBook Pro", ex.Message);
        }

        [Fact]
        public async Task UpdateFnFStatusAsync_WhenPendingAssetsWithOverride_Succeeds()
        {
            var res = new Resignation
            {
                Id = 1,
                EmployeeId = 50,
                ResignationStatus = "ExitClearance",
                FnFStatus = "Pending",
                IsDeleted = false
            };

            _resRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(res);

            _checklistMock.Setup(c => c.VerifyAssetReturnsAsync(50))
                .ReturnsAsync(new AssetReturnVerificationDto
                {
                    AllCleared = false,
                    PendingAssets = 1,
                    PendingAssetDetails = new List<string> { "Lost Access Card (AST-042)" }
                });

            var dto = new FnFStatusUpdateDto
            {
                Status = "Prepared",
                Remarks = "Deducted ₹500 from salary",
                OverrideReason = "Card replacement fee deducted from final settlement"
            };

            var result = await _service.UpdateFnFStatusAsync(1, dto);
            Assert.True(result);
            Assert.Equal("Prepared", res.FnFStatus);
            Assert.Equal("Card replacement fee deducted from final settlement", res.AssetClearanceOverrideReason);
            _resRepoMock.Verify(r => r.Update(res), Times.Once);
        }

        [Fact]
        public async Task UpdateFnFStatusAsync_WhenAllAssetsCleared_SucceedsWithoutOverride()
        {
            var res = new Resignation
            {
                Id = 1,
                EmployeeId = 50,
                ResignationStatus = "ExitClearance",
                FnFStatus = "Pending",
                IsDeleted = false
            };

            _resRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(res);

            _checklistMock.Setup(c => c.VerifyAssetReturnsAsync(50))
                .ReturnsAsync(new AssetReturnVerificationDto
                {
                    AllCleared = true,
                    PendingAssets = 0
                });

            var dto = new FnFStatusUpdateDto
            {
                Status = "Settled",
                Remarks = "Payment disbursed via NEFT"
            };

            var result = await _service.UpdateFnFStatusAsync(1, dto);
            Assert.True(result);
            Assert.Equal("Settled", res.FnFStatus);
            _resRepoMock.Verify(r => r.Update(res), Times.Once);
        }
    }
}
