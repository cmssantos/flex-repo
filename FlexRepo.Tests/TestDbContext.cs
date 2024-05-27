using Microsoft.EntityFrameworkCore;

namespace FlexRepo.Tests;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestUser> TestUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestUser>().HasKey(u => u.Id);
    }
}