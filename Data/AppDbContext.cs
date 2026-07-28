using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Models;

namespace PhotographyCMS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options
    ) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<BlogImage> BlogImages => Set<BlogImage>();
    public DbSet<HeroSlide> HeroSlides { get; set; }
    public DbSet<AwardPhoto> AwardPhotos { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<GalleryItem> GalleryItems { get; set; }
    public DbSet<GalleryCategory> GalleryCategories { get; set; }
    public DbSet<Award> Awards { get; set; }
    public DbSet<AboutStat> AboutStats { get; set; }
    public DbSet<GearItem> GearItems { get; set; }
    public DbSet<AboutHero> AboutHeroes { get; set; }
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GalleryCategory>().HasData(
            new GalleryCategory { Id = 1, Name = "Award-winning Gallery", Slug = "award-winning", Description = "Discover standout work that has earned recognition and acclaim." },
            new GalleryCategory { Id = 2, Name = "Internationally Qualified", Slug = "internationally-qualified", Description = "Explore work that has been recognized across international platforms." },
            new GalleryCategory { Id = 3, Name = "Recent Work", Slug = "recent-work", Description = "View the latest projects and the newest additions to the portfolio." }
        );

        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Images)
            .WithOne(i => i.Blog)
            .HasForeignKey(i => i.BlogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Blog)
            .WithMany()
            .HasForeignKey(c => c.BlogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}