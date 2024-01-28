using Miniblog.Domain;
using Miniblog.UseCases.Abstract;

namespace Miniblog.Infrastructure.DataAccess.Repositories;

using Microsoft.EntityFrameworkCore;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly BlogDbContext _blogDbContext;

    public CategoryRepository(BlogDbContext blogDbContext)
    {
        this._blogDbContext = blogDbContext;
    }

    public Task SaveAsync(Category category, CancellationToken cancellationToken)
    {
        var entityEntry = this._blogDbContext.Entry(category);

        if (entityEntry.State == EntityState.Detached)
        {
            this._blogDbContext.Categories.Add(category);
        }

        return this._blogDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken) =>
        this._blogDbContext.Categories.ToListAsync(cancellationToken);
}
