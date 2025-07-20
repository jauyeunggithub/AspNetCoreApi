using System.Security.Claims;      // For working with Claims in tests
using Microsoft.AspNetCore.Http;    // For DefaultHttpContext
using Microsoft.AspNetCore.Mvc;     // For OkObjectResult, NotFoundObjectResult, CreatedAtActionResult, ControllerContext, etc.
using Microsoft.Extensions.Logging; // For logging (if needed in tests)
using Microsoft.AspNetCore.Identity; // For UserManager, IUserStore, ApplicationUser, etc.
using Moq;          // For creating mock objects
using System.Threading.Tasks;       // For async/await functionality in tests
using Xunit;        // For Fact attribute and unit test framework
using AspNetCoreApi.Controllers;    // For EventController
using AspNetCoreApi.Models;         // For ApplicationUser, Event, ConferenceEvent, WebinarEvent
using AspNetCoreApi.Data;           // For ApplicationDbContext
using System;       // For handling system-level types like Exception and Task, Guid
using System.Collections.Generic;   // For IEnumerable and List
using Microsoft.EntityFrameworkCore; // For UseInMemoryDatabase, DbContextOptionsBuilder
using System.Linq; // For .Count()

public class EventControllerTests : IDisposable // Implement IDisposable for cleanup
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly EventController _controller;

    public EventControllerTests()
    {
        // Setup in-memory database options
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique name for each test run to ensure isolation
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureDeleted(); // Ensure a clean state for each test suite
        _context.Database.EnsureCreated(); // Create the database

        // Mock the UserManager<ApplicationUser> instance for UserManager dependency
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), // Simplifies the store mock
            null, null, null, null, null, null, null, null);

        _controller = new EventController(_context);

        // --- FIX START: Setting up HttpContext for OwnerId claim ---
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

    // Cleanup method to ensure a clean state between tests, especially for in-memory DB
    // This is good practice for tests that modify the database
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateEvent_ReturnsCreatedResult_WhenEventIsValid()
    {
        // Arrange
        // Use a concrete class that inherits from Event. Assuming ConferenceEvent.
        // Ensure 'Location' is provided if it's a required property for ConferenceEvent.
        var newEvent = new ConferenceEvent
        {
            Title = "Test Conference",
            Location = "Online", // Provide Location for ConferenceEvent
            Url = "http://testconference.com"
            // OwnerId will be set by the controller based on claims
        };

        // Act
        var result = await _controller.CreateEvent(newEvent);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<Event>(createdResult.Value); // Use IsAssignableFrom for abstract base type

        Assert.Equal("Test Conference", returnValue.Title);
        Assert.Equal("Online", ((ConferenceEvent)returnValue).Location); // Cast to ConferenceEvent to access Location
        Assert.Equal("123", returnValue.OwnerId); // Verify that the OwnerId was correctly assigned from claims
        Assert.Equal("http://testconference.com", returnValue.Url); // Verify Url
    }

    [Fact]
    public async Task GetUserEvents_ReturnsOkResult_WhenUserHasEvents()
    {
        // Arrange
        var userId = "123"; // Matches the mocked ClaimTypes.NameIdentifier

        // Add events for the mocked user, ensuring concrete types and required properties are set
        _context.Events.Add(new ConferenceEvent { Title = "User's Conference", Location = "Conference Hall", Url = "http://userconference.com", OwnerId = userId });
        _context.Events.Add(new WebinarEvent { Title = "User's Webinar", Url = "http://userwebinar.com", OwnerId = userId });
        _context.Events.Add(new ConferenceEvent { Title = "Other User's Event", Location = "Other Location", Url = "http://otherconference.com", OwnerId = "another_user_id" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);

        Assert.NotEmpty(returnValue);
        Assert.Equal(2, returnValue.Count()); // Expecting 2 events for user "123"
        Assert.All(returnValue, e => Assert.Equal(userId, e.OwnerId)); // All returned events belong to the mocked user
    }

    [Fact]
    public async Task GetUserEvents_ReturnsEmpty_WhenUserHasNoEvents()
    {
        // Arrange - No events added for userId "123" in this test

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
        // Use a concrete type and provide all required properties
        var eventItem = new ConferenceEvent { Title = "Specific Conference", Location = "Venue A", Url = "http://specificconference.com", OwnerId = userId };
        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetEventById(eventItem.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<Event>(okResult.Value); // Assert against abstract base type

        Assert.Equal(eventItem.Title, returnValue.Title);
        Assert.Equal(userId, returnValue.OwnerId); // Ensure the returned event belongs to the correct user

        // If you need to assert specific properties of the derived type, cast it
        var conferenceEvent = Assert.IsType<ConferenceEvent>(returnValue);
        Assert.Equal("Venue A", conferenceEvent.Location);
    }

    [Fact]
    public async Task GetEventById_ReturnsNotFound_WhenEventDoesNotExist()
    {
        // Arrange - No event 999 exists in the in-memory DB

        // Act
        var result = await _controller.GetEventById(999); // Non-existing event ID

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
