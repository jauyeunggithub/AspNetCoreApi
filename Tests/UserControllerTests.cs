using System.Security.Claims; // For working with Claims in tests
using Microsoft.AspNetCore.Http; // For IHttpContextAccessor and DefaultHttpContext
using Microsoft.AspNetCore.Mvc; // For OkObjectResult, NotFoundObjectResult, ControllerContext, etc.
using Microsoft.Extensions.Logging; // For logging (if needed in tests)
using Microsoft.AspNetCore.Identity; // For UserManager, IUserStore, ApplicationUser, etc.
using Moq; // For creating mock objects
using System.Threading.Tasks; // For async/await functionality in tests
using Xunit; // For Fact attribute and unit test framework
using AspNetCoreApi.Controllers; // For UserController
using AspNetCoreApi.Models; // For ApplicationUser (or any related model in your project)
using System; // For handling system-level types like Exception and Task
using System.Collections.Generic; // For List<Claim>

public class UserControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    // Removed _mockHttpContextAccessor as it's not needed for this approach
    private readonly UserController _controller;

    public UserControllerTests()
    {
        // Mock the IUserStore<ApplicationUser> dependency
        var store = new Mock<IUserStore<ApplicationUser>>();

        // Mock UserManager<ApplicationUser> constructor
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object,
                                                                 null,
                                                                 null,
                                                                 null,
                                                                 null,
                                                                 null,
                                                                 null,
                                                                 null,
                                                                 null);

        // Initialize the UserController
        _controller = new UserController(_mockUserManager.Object);

        // --- FIX START ---
        // Create a ClaimsPrincipal with the desired NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"), // The user ID you want to simulate
            // Add other claims if your controller logic depends on them
            // new Claim(ClaimTypes.Name, "testuser@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Set the ControllerContext's HttpContext.User directly
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        // --- FIX END ---
    }

    [Fact]
    public async Task GetProfile_ReturnsOkResult_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser { Id = "123", UserName = "testuser", Email = "test@domain.com" };

        // Set up UserManager mock to return the user when calling FindByIdAsync
        _mockUserManager.Setup(m => m.FindByIdAsync("123")).ReturnsAsync(user);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        // Check if the result is an ActionResult<ApplicationUser> or just ActionResult
        // Adjust based on your actual controller method signature
        // If your controller returns ActionResult<ApplicationUser> as indicated by your method signature:
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ApplicationUser>(okResult.Value);
        Assert.Equal("testuser", returnValue.UserName); // Validate that the returned user matches
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        // Set up UserManager mock to return null when calling FindByIdAsync
        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        // Check if the result is an ActionResult<ApplicationUser> or just ActionResult
        // If your controller returns ActionResult<ApplicationUser> as indicated by your method signature:
        Assert.IsType<NotFoundObjectResult>(result.Result); // Ensure the result is NotFound
    }
}
