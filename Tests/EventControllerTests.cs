using System.Security.Claims;                  // For working with Claims in tests
using Microsoft.AspNetCore.Http;               // For IHttpContextAccessor and DefaultHttpContext
using Microsoft.AspNetCore.Mvc;               // For OkObjectResult, NotFoundObjectResult, ControllerContext, etc.
using Microsoft.Extensions.Logging;            // For logging (if needed in tests)
using Microsoft.AspNetCore.Identity;           // For UserManager, IUserStore, ApplicationUser, etc.
using Moq;                                     // For creating mock objects
using System.Threading.Tasks;                  // For async/await functionality in tests
using Xunit;                                   // For Fact attribute and unit test framework
using AspNetCoreApi.Controllers;               // For UserController
using AspNetCoreApi.Models;                    // For ApplicationUser (or any related model in your project)
using AspNetCoreApi.Data;
using System;                                  // For handling system-level types like Exception and Task
using AspNetCoreApi.Tests.TestModels;          // For ConcreteEvent and other test models
using Microsoft.EntityFrameworkCore;

public class EventControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly EventController _controller;

    public EventControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb") // In-memory database for testing
            .Options;

        _context = new ApplicationDbContext(options);

        // Mock the UserManager<ApplicationUser> instance for UserManager dependency
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _controller = new EventController(_context);  // Inject the mocked UserManager

        // Mock the IHttpContextAccessor (to mock the current authenticated user)
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.Setup(m => m.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)).Returns("123");  // Mock user ID

        // Mock the IServiceProvider to return IHttpContextAccessor
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor)))
                           .Returns(_mockHttpContextAccessor.Object);

        // Set the HttpContext.RequestServices to be the mocked IServiceProvider
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object }
        };
    }

    [Fact]
    public async Task CreateEvent_ReturnsCreatedResult_WhenEventIsValid()
    {
        // Arrange
        var newEvent = new ConcreteEvent("Test Event", "Test Location", "http://test.com", "123");

        // Act
        var result = await _controller.CreateEvent(newEvent);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<Event>(createdResult.Value);
        Assert.Equal("Test Event", returnValue.Title);
    }

    [Fact]
    public async Task GetUserEvents_ReturnsOkResult_WhenUserHasEvents()
    {
        // Arrange
        var userId = "123";
        _context.Events.Add(new ConcreteEvent("User's Event", "User's Location", "http://user.com", userId));
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);
        Assert.NotEmpty(returnValue);
    }

    [Fact]
    public async Task GetUserEvents_ReturnsEmpty_WhenUserHasNoEvents()
    {
        // Arrange
        var userId = "123";

        // Act
        var result = await _controller.GetUserEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);
        Assert.Empty(returnValue);
    }

    [Fact]
    public async Task GetEventById_ReturnsOkResult_WhenEventExists()
    {
        // Arrange
        var userId = "123";
        var eventItem = new ConcreteEvent("User's Event", "User's Location", "http://user.com", userId);
        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetEventById(eventItem.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<Event>(okResult.Value);
        Assert.Equal(eventItem.Title, returnValue.Title);
    }

    [Fact]
    public async Task GetEventById_ReturnsNotFound_WhenEventDoesNotExist()
    {
        // Act
        var result = await _controller.GetEventById(999);  // Non-existing event ID

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
