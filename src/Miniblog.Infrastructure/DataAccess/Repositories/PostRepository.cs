using Miniblog.Domain;
using Miniblog.UseCases.Abstract;

namespace Miniblog.Infrastructure.DataAccess.Repositories;

using Microsoft.EntityFrameworkCore;

internal sealed class PostRepository : IPostRepository
{
    private readonly BlogDbContext _blogDbContext;

    public PostRepository(BlogDbContext blogDbContext) => _blogDbContext = blogDbContext;

    public Task SaveAsync(Post post, CancellationToken cancellationToken)
    {
        var entityEntry = _blogDbContext.Entry(post);

        if (entityEntry.State == EntityState.Detached)
        {
            _blogDbContext.Posts.Add(post);
        }

        return _blogDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Post post, CancellationToken cancellationToken)
    {
        _blogDbContext.Posts.Remove(post);
        return _blogDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Post?> FindByIdAsync(string id, CancellationToken cancellationToken) =>
        _blogDbContext.Posts
            .Include(p => p.Comments)
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Post?> FindBySlugAsync(string slug, CancellationToken cancellationToken) => this
        ._blogDbContext.Posts
        .Include(p => p.Comments)
        .Include(p => p.Categories)
        .Include(p => p.Tags)
        .SingleOrDefaultAsync(p => p.Slug == slug, cancellationToken);


    public Task<List<Post>> FindAllByTagAsync(int skip, int take, string tag,
        CancellationToken cancellationToken) => _blogDbContext.Posts
        .Include(p => p.Comments)
        .Include(p => p.Categories)
        .Include(p => p.Tags)
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
        _blogDbContext.Posts
            .Include(p => p.Comments)
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Where(p => p.Categories.Any(c => c.Name == category))
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountByCategoryAsync(string category, CancellationToken cancellationToken) =>
        _blogDbContext.Posts
            .Where(p => p.Categories.Any(c => c.Name == category))
            .CountAsync(cancellationToken);

    public Task<List<Post>> FindAllAsync(int skip, int take, CancellationToken cancellationToken) =>
        _blogDbContext.Posts
            .Include(p => p.Comments)
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAllAsync(CancellationToken cancellationToken) => _blogDbContext.Posts
        .CountAsync(cancellationToken);
}
