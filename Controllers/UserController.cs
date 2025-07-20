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
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // All endpoints in this controller require JWT authentication
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Get User Profile
        [HttpGet("profile")]
        public async Task<ActionResult<ApplicationUser>> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        // Update User Profile
        [HttpPut("profile")]
        public async Task<ActionResult<ApplicationUser>> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found.");

            user.UserName = model.UserName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(user);
        }
    }

    public class UpdateProfileModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
