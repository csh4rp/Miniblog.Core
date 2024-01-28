using Miniblog.Domain;
using Miniblog.UseCases.Abstract;

namespace Miniblog.Infrastructure.DataAccess.Repositories;

using Microsoft.EntityFrameworkCore;

public class TagRepository : ITagRepository
{
    private readonly BlogDbContext _blogDbContext;

    public TagRepository(BlogDbContext blogDbContext) => _blogDbContext = blogDbContext;

    public Task SaveAsync(Tag tag, CancellationToken cancellationToken)
    {
        var entityEntry = _blogDbContext.Entry(tag);

        if (entityEntry.State == EntityState.Detached)
        {
            _blogDbContext.Tags.Add(tag);
        }

        return _blogDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken) =>
        _blogDbContext.Tags.ToListAsync(cancellationToken);
}
