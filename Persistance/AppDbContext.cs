using Microsoft.EntityFrameworkCore;
using PhotoAiBackend.Persistance.Entities;

namespace PhotoAiBackend.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ImageJob> ImageJobs { get; set; }
    public DbSet<EnhanceJob> EnhanceJobs { get; set; }
    public DbSet<EnhanceImage> EnhanceImages { get; set; }
}