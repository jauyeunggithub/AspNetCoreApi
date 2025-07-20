// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace AspNetCoreApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties here if needed
        public string FullName { get; set; }
    }
}
