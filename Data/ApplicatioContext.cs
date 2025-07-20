// Namespace for the Data folder, ensure it matches your folder structure
namespace AspNetCoreApi.Data
{
    using Microsoft.EntityFrameworkCore;
    using AspNetCoreApi.Models;

    public class ApplicationDbContext : DbContext
    {
        // Constructor to pass options to the base DbContext class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet properties for each of your entities
        public DbSet<Event> Events { get; set; }
        public DbSet<ConferenceEvent> ConferenceEvents { get; set; }
        public DbSet<WebinarEvent> WebinarEvents { get; set; }
    }
}
