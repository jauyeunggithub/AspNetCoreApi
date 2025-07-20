// Tests/EventsControllerTests.cs
public class EventsControllerTests
{
    [Fact]
    public async Task GetEvents_ReturnsAllEvents()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var context = new ApplicationDbContext(options);
        var controller = new EventsController(context);

        // Act
        var result = await controller.GetEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);
        Assert.NotEmpty(returnValue);
    }
}
