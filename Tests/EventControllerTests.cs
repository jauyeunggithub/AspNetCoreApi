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

// NOTE: You mentioned AspNetCoreApi.Tests.TestModels; // For ConcreteEvent and other test models
// If ConcreteEvent is a separate concrete class specifically for testing, ensure it inherits from Event
// For simplicity, I will use ConferenceEvent or WebinarEvent from your models.
// If ConcreteEvent is defined elsewhere and acts as the concrete Event for tests,
// ensure it has all required properties like Title, Date, OwnerId, Location, Url.
// Let's assume ConcreteEvent is defined as:
/*
namespace AspNetCoreApi.Tests.TestModels
{
    public class ConcreteEvent : AspNetCoreApi.Models.Event // Must inherit from Event
    {
        // No additional properties are strictly needed here unless you shadow base properties
        // or add test-specific ones.
        // If your controller's CreateEvent action expects a DTO or specific constructor,
        // you might need to adjust this.
        // For the purpose of resolving compilation errors, we just need a concrete type.
    }
}
*/


public class EventControllerTests : IDisposable // Implement IDisposable for cleanup
{
    private readonly ApplicationDbContext _context;
    // _mockUserManager is correctly set up for UserManager dependency.
    // If EventController doesn't directly use UserManager, this mock might not be strictly needed by controller tests.
    // However, it's good practice if other parts of the system interact with it.
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

        // Initialize the EventController
        // IMPORTANT: Ensure your EventController's constructor correctly handles its dependencies.
        // If it takes UserManager, you might need to pass _mockUserManager.Object here.
        // Based on your original code, it seems to only take ApplicationDbContext.
        _controller = new EventController(_context);

        // --- FIX START: Setting up HttpContext for UserId (now OwnerId) claim ---
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
        // The userId will be pulled from HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
        // which we've set to "123" in the constructor.

        // FIX CS0144: Cannot create an instance of the abstract type 'Event'
        // FIX CS0117: 'Event' does not contain a definition for 'Link' (now 'Url')
        // FIX CS0117: 'Event' does not contain a definition for 'UserId' (now 'OwnerId')
        // Use a concrete class that inherits from Event. Assuming ConferenceEvent or WebinarEvent exists.
        // If you specifically have a 'ConcreteEvent' in TestModels, use that,
        // but ensure it properly inherits from AspNetCoreApi.Models.Event
        var newEvent = new ConferenceEvent // Using ConferenceEvent as an example concrete type
        {
            Title = "Test Event",
            Location = "Test Location",
            Url = "http://test.com" // Changed from Link to Url
            // OwnerId is set by the controller based on the authenticated user claims,
            // so we don't set it here in the input model for the controller's CreateEvent.
            // If your CreateEvent method takes a DTO that includes UserId/OwnerId,
            // then you'd set it here, but generally, it's safer to pull from claims in the controller.
        };

        // Act
        // Assuming your CreateEvent takes a model that doesn't include OwnerId, and the controller extracts it
        var result = await _controller.CreateEvent(newEvent);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        // FIX xUnit2018: Do not compare an object's exact type to the abstract class 'AspNetCoreApi.Models.Event'.
        // Use Assert.IsAssignableFrom if you want to check if it's an Event or derived type.
        // If you expect the specific concrete type (e.g., ConferenceEvent), use Assert.IsType<ConferenceEvent>.
        var returnValue = Assert.IsAssignableFrom<Event>(createdResult.Value);

        Assert.Equal("Test Event", returnValue.Title);
        // FIX CS1061: 'Event' does not contain a definition for 'UserId' (now 'OwnerId')
        Assert.Equal("123", returnValue.OwnerId); // Verify that the OwnerId was correctly assigned from claims
    }

    [Fact]
    public async Task GetUserEvents_ReturnsOkResult_WhenUserHasEvents()
    {
        // Arrange
        var userId = "123"; // Matches the mocked ClaimTypes.NameIdentifier

        // FIX CS0144: Cannot create an instance of the abstract type 'Event'
        // FIX CS0117: 'Event' does not contain a definition for 'Link' (now 'Url')
        // FIX CS0117: 'Event' does not contain a definition for 'UserId' (now 'OwnerId')
        _context.Events.Add(new ConferenceEvent { Title = "User's Event", Location = "User's Location", Url = "http://user.com", OwnerId = userId });
        _context.Events.Add(new WebinarEvent { Title = "Another User's Event", Url = "http://user2.com", OwnerId = userId }); // Add another to test multiple
        _context.Events.Add(new ConferenceEvent { Title = "Other User's Event", Location = "Other Location", Url = "http://other.com", OwnerId = "another_user_id" }); // Event for a different user
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // FIX xUnit2018: Use IsAssignableFrom for IEnumerable of abstract type
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);

        Assert.NotEmpty(returnValue);
        // Assert.Single(returnValue); // This might fail if you add more events as I did above.
        // A better check is to count or filter.
        Assert.Equal(2, returnValue.Count()); // Assuming 2 events for user "123" were added
        // FIX CS1061: 'Event' does not contain a definition for 'UserId' (now 'OwnerId')
        Assert.All(returnValue, e => Assert.Equal(userId, e.OwnerId)); // All returned events belong to the mocked user
    }

    [Fact]
    public async Task GetUserEvents_ReturnsEmpty_WhenUserHasNoEvents()
    {
        // Arrange
        // No events added for userId "123" in this test, implicitly.
        // Let's ensure no events for this user ID exist from previous tests.
        // (EnsureDeleted/EnsureCreated in constructor already handles this for fresh context).

        // Act
        var result = await _controller.GetUserEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // FIX xUnit2018: Use IsAssignableFrom for IEnumerable of abstract type
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);
        Assert.Empty(returnValue);
    }

    [Fact]
    public async Task GetEventById_ReturnsOkResult_WhenEventExists()
    {
        // Arrange
        var userId = "123";
        // FIX CS0144: Cannot create an instance of the abstract type 'Event'
        // FIX CS0117: 'Event' does not contain a definition for 'Link' (now 'Url')
        // FIX CS0117: 'Event' does not contain a definition for 'UserId' (now 'OwnerId')
        var eventItem = new ConferenceEvent { Title = "User's Event", Location = "User's Location", Url = "http://user.com", OwnerId = userId };
        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetEventById(eventItem.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // FIX xUnit2018: This specific test expects the *exact* concrete type if the controller returns it.
        // If your controller's GetEventById returns a concrete type, keep Assert.IsType<ConferenceEvent>.
        // If it returns 'Event', but the actual object is a ConferenceEvent, Assert.IsAssignableFrom<Event> is safer.
        // Assuming your controller might return the base type 'Event' here, use IsAssignableFrom.
        var returnValue = Assert.IsAssignableFrom<Event>(okResult.Value);
        Assert.Equal(eventItem.Title, returnValue.Title);
        // FIX CS1061: 'Event' does not contain a definition for 'UserId' (now 'OwnerId')
        Assert.Equal(userId, returnValue.OwnerId); // Ensure the returned event belongs to the correct user
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
