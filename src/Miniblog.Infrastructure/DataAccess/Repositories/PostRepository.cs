namespace Miniblog.Infrastructure.DataAccess.Repositories;

using Domain.Abstract;
using Domain.Models;

using Microsoft.EntityFrameworkCore;

public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _blogDbContext;

    public PostRepository(BlogDbContext blogDbContext)
    {
        this._blogDbContext = blogDbContext;
    }

    public Task SaveAsync(Post post, CancellationToken cancellationToken)
    {
        var entityEntry = this._blogDbContext.Entry(post);

        if (entityEntry.State == EntityState.Detached)
        {
            this._blogDbContext.Posts.Add(post);
        }

        return this._blogDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Post post, CancellationToken cancellationToken)
    {
        this._blogDbContext.Posts.Remove(post);
        return this._blogDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Post?> FindByIdAsync(string id, CancellationToken cancellationToken) =>
        this._blogDbContext.Posts
            .Include(p => p.Comments)
            .Include(p => p.Categories)
            .Include(p => p.Comments)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Post?> FindBySlugAsync(string slug, CancellationToken cancellationToken) => this
        ._blogDbContext.Posts
        .Include(p => p.Comments)
        .Include(p => p.Categories)
        .Include(p => p.Comments)
        .SingleOrDefaultAsync(p => p.Slug == slug, cancellationToken);


    public Task<List<Post>> FindAllByTagAsync(int skip, int take, string tag,
        CancellationToken cancellationToken) => this._blogDbContext.Posts
        .Include(p => p.Comments)
        .Include(p => p.Categories)
        .Include(p => p.Comments)
        .Where(p => p.Tags.Any(t => t.Name == tag))
        .OrderBy(p => p.Id)
        .Skip(skip)
        .Take(take)
        .ToListAsync(cancellationToken);

    public Task<int> CountByTagAsync(string tag, CancellationToken cancellationToken) => this
        ._blogDbContext.Posts
        .Where(p => p.Tags.Any(t => t.Name == tag))
        .CountAsync(cancellationToken);

    public Task<List<Post>> FindAllByCategoryAsync(int skip, int take, string category,
        CancellationToken cancellationToken) =>
        this._blogDbContext.Posts
            .Include(p => p.Comments)
            .Include(p => p.Categories)
            .Include(p => p.Comments)
            .Where(p => p.Categories.Any(c => c.Name == category))
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountByCategoryAsync(string category, CancellationToken cancellationToken) =>
        this._blogDbContext.Posts
            .Where(p => p.Categories.Any(c => c.Name == category))
            .CountAsync(cancellationToken);

    public Task<List<Post>> FindAllAsync(int skip, int take, CancellationToken cancellationToken) =>
        this._blogDbContext.Posts
            .Include(p => p.Comments)
            .Include(p => p.Categories)
            .Include(p => p.Comments)
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAllAsync(CancellationToken cancellationToken) => this._blogDbContext.Posts
        .CountAsync(cancellationToken);
}
