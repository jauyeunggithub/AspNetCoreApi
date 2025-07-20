using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Event> Events { get; set; }
    public DbSet<ConferenceEvent> ConferenceEvents { get; set; }
    public DbSet<WebinarEvent> WebinarEvents { get; set; }
}
