using System.Security.Claims;                        // For working with Claims in tests
using Microsoft.AspNetCore.Http;                    // For IHttpContextAccessor and DefaultHttpContext
using Microsoft.AspNetCore.Mvc;                    // For OkObjectResult, NotFoundObjectResult, ControllerContext, etc.
using Microsoft.Extensions.Logging;                 // For logging (if needed in tests)
using Microsoft.AspNetCore.Identity;                // For UserManager, IUserStore, ApplicationUser, etc.
using Moq;                                          // For creating mock objects
using System.Threading.Tasks;                       // For async/await functionality in tests
using Xunit;                                        // For Fact attribute and unit test framework
using AspNetCoreApi.Controllers;                    // For UserController
using AspNetCoreApi.Models;                         // For ApplicationUser (or any related model in your project)
using System;                                       // For handling system-level types like Exception and Task

public class UserControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.Setup(m => m.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)).Returns("123");  // Mock the user ID

        _controller = new UserController(_mockUserManager.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() { RequestServices = _mockHttpContextAccessor.Object }
        };
    }

    [Fact]
    public async Task GetProfile_ReturnsOkResult_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser { Id = "123", UserName = "testuser", Email = "test@domain.com" };
        _mockUserManager.Setup(m => m.FindByIdAsync("123")).ReturnsAsync(user);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ApplicationUser>(okResult.Value);
        Assert.Equal("testuser", returnValue.UserName);
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
