using Miniblog.Domain;

namespace Miniblog.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Post>()
            .HasMany(b => b.Tags)
            .WithMany();

        modelBuilder.Entity<Post>()
            .HasMany(b => b.Categories)
            .WithMany();

        modelBuilder.Entity<Post>()
            .HasMany(b => b.Comments)
            .WithOne()
            .HasForeignKey(b => b.PostId);

        modelBuilder.Entity<Comment>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Tag>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Category>()
            .HasKey(b => b.Id);
    }
}
