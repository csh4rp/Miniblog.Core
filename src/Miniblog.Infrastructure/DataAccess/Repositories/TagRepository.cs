namespace Miniblog.Infrastructure.DataAccess.Repositories;

using Domain.Abstract;
using Domain.Models;

using Microsoft.EntityFrameworkCore;

public class TagRepository : ITagRepository
{
    private readonly BlogDbContext _blogDbContext;

    public TagRepository(BlogDbContext blogDbContext)
    {
        this._blogDbContext = blogDbContext;
    }

    public Task SaveAsync(Tag tag, CancellationToken cancellationToken)
    {
        var entityEntry = this._blogDbContext.Entry(tag);

        if (entityEntry.State == EntityState.Detached)
        {
            this._blogDbContext.Tags.Add(tag);
        }

        return this._blogDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken) =>
        this._blogDbContext.Tags.ToListAsync(cancellationToken);
}
