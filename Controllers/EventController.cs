namespace AspNetCoreApi.Controllers
{
    using Microsoft.AspNetCore.Authorization;  // For [Authorize] attribute
    using Microsoft.AspNetCore.Mvc;          // For [ApiController], [HttpGet], [HttpPost], etc.
    using Microsoft.EntityFrameworkCore;      // For Entity Framework Core
    using System.Collections.Generic;        // For List<T>
    using System.Linq;                       // For LINQ methods like Where()
    using System.Security.Claims;            // For User.FindFirstValue
    using System.Threading.Tasks;            // For async Task
    using AspNetCoreApi.Data;                // For ApplicationDbContext
    using AspNetCoreApi.Models;              // Assuming your Event model is here
    using Microsoft.EntityFrameworkCore;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // All endpoints in this controller require JWT authentication
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create Event
        [HttpPost("create")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] Event newEvent)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (newEvent == null)
                return BadRequest("Event data is invalid.");

            // Attach the creator (for simplicity, assuming this event has an "OwnerId" field)
            newEvent.OwnerId = userId;

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }

        // Get Events by User
        [HttpGet("my-events")]
        public async Task<ActionResult<IEnumerable<Event>>> GetUserEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userEvents = await _context.Events
                .Where(e => e.OwnerId == userId)
                .ToListAsync();

            return Ok(userEvents);
        }

        // Get Event by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            return Ok(eventItem);
        }
    }
}
