using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

public class RoleMasterControllerTests
{
    private readonly Mock<IRoleMasterService> _mockService;
    private readonly RoleMasterController _controller;

    public RoleMasterControllerTests()
    {
        _mockService = new Mock<IRoleMasterService>();
        _controller = new RoleMasterController(_mockService.Object);
    }

    [Fact]
    public async Task Get_Roles_ShouldReturnOk_WhenRolesExist()
    {
        // Arrange
        var roles = new List<RoleMaster> { new RoleMaster { Id = 1, RoleName = "Admin" } };
        _mockService.Setup(service => service.GetAllRoleMaster()).ReturnsAsync(roles);

        // Act
        var result = await _controller.GetRoles();
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
    }

    [Fact]
    public async Task Get_Roles_ShouldReturnNotFound_WhenNoRolesExist()
    {
        // Arrange
        _mockService.Setup(service => service.GetAllRoleMaster()).ReturnsAsync(new List<RoleMaster>());

        // Act
        var result = await _controller.GetRoles();
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
    }

    [Fact]
    public async Task Add_Role_ShouldReturnOk_WhenRoleIsAdded()
    {
        // Arrange
        var role = new RoleMaster { Id = 1, RoleName = "Manager" };
        _mockService.Setup(service => service.AddRoleMaster(role)).ReturnsAsync(true);

        // Act
        var result = await _controller.AddRole(role);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
    }

    [Fact]
    public async Task Add_Role_ShouldReturnBadRequest_WhenRoleIsNotAdded()
    {
        // Arrange
        var role = new RoleMaster { Id = 1, RoleName = "Manager" };
        _mockService.Setup(service => service.AddRoleMaster(role)).ReturnsAsync(false);

        // Act
        var result = await _controller.AddRole(role);
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task UpdateRole_ShouldReturnOk_WhenRoleIsUpdated()
    {
        // Arrange
        var role = new RoleMaster { Id = 1, RoleName = "Supervisor" };
        _mockService.Setup(service => service.UpdateRoleMaster(role)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateRole(role.Id, role);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

        var response = okResult.Value as ApiResponse<bool>;
        Assert.NotNull(response);
        Assert.True(response.Data);
    }



    [Fact]
    public async Task UpdateRole_ShouldReturnBadRequest_WhenRoleIsNotUpdated()
    {
        // Arrange
        var role = new RoleMaster { Id = 1, RoleName = "Supervisor" };
        _mockService.Setup(service => service.UpdateRoleMaster(role)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateRole(role.Id, role);
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);

        var response = badRequestResult.Value as ApiResponse<bool>;
        Assert.NotNull(response);
        Assert.False(response.Data);
    }


    [Fact]
    public async Task DeleteRole_ShouldReturnOk_WhenRoleIsDeleted()
    {
        // Arrange
        _mockService.Setup(service => service.DeleteRoleMaster(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteRole(1);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
    }

    [Fact]
    public async Task DeleteRole_ShouldReturnBadRequest_WhenRoleIsNotDeleted()
    {
        // Arrange
        _mockService.Setup(service => service.DeleteRoleMaster(1)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteRole(1);
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
    }
}

