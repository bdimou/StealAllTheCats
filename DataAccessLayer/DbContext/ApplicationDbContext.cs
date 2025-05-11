using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;


namespace DataAccessLayer.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Cat> Cats { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<CatTag> CatTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure many-to-many relationship between Cat and Tag through CatTag  
        modelBuilder.Entity<CatTag>()
            .HasKey(ct => new { ct.CatId, ct.TagId });

        modelBuilder.Entity<CatTag>()
            .HasOne(ct => ct.Cat)
            .WithMany(c => c.CatTags) // Corrected navigation property  
            .HasForeignKey(ct => ct.CatId);

        modelBuilder.Entity<CatTag>()
            .HasOne(ct => ct.Tag)
            .WithMany(t => t.CatTags)
            .HasForeignKey(ct => ct.TagId);

        base.OnModelCreating(modelBuilder);
    }
}
